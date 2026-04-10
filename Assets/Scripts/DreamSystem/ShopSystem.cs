using System.Collections.Generic;
using System.Linq;
using Data;
using DreamConfig;
using DreamManager;
using DreamSystem.Damage;
using DreamSystem.Damage.Stat;
using Enum.Buff;
using Enum.Item;
using Model.Player;
using Struct;
using UnityEngine;
using VContainer;

namespace DreamSystem
{
    /// <summary>
    /// 负责商店商品生成与购买结算。
    /// </summary>
    public class ShopSystem
    {
        private readonly GameSessionData _sessionData;
        private readonly ResourcesManager _resources;
        private readonly BuffSystem _buffSystem;
        private readonly PlayerModel _playerModel;

        private readonly List<ShopOffer> _currentOffers = new();

        [Inject]
        public ShopSystem(GameSessionData sessionData, ResourcesManager resources, BuffSystem buffSystem, PlayerModel playerModel)
        {
            _sessionData = sessionData;
            _resources = resources;
            _buffSystem = buffSystem;
            _playerModel = playerModel;
        }

        public List<ShopOffer> BuildOffersForChapter(int chapter, int itemOfferCount = 3)
        {
            _currentOffers.Clear();

            var itemConfig = _resources.GetConfig<ItemConfig>();
            if (itemConfig == null)
            {
                // Debug.LogWarning("[ShopSystem] ItemConfig 未就绪，商店无可售卖道具。");
                AddPotionOffer(chapter);
                return _currentOffers;
            }

            var candidates = itemConfig.GetAllItemList()
                .Where(CanAppearInShop)
                .ToList();

            var selected = WeightedSampleWithoutReplacement(candidates, itemOfferCount);
            for (int i = 0; i < selected.Count; i++)
            {
                var item = selected[i];
                _currentOffers.Add(new ShopOffer
                {
                    OfferId = $"item_{i}_{item.itemId}",
                    IsPotion = false,
                    ItemId = item.itemId,
                    DisplayName = item.itemName,
                    Description = item.description,
                    IconPath = string.IsNullOrWhiteSpace(item.iconPath) ? item.itemId : item.iconPath,
                    Price = CalculateItemPrice(item.itemRarity, chapter),
                    IsSold = false,
                    HealPercent = 0f
                });
            }

            AddPotionOffer(chapter);
            return _currentOffers;
        }

        public List<ShopOffer> GetCurrentOffers() => _currentOffers;

        public bool TryPurchase(string offerId, out string message)
        {
            var offer = _currentOffers.FirstOrDefault(x => x.OfferId == offerId);
            if (offer == null)
            {
                message = "商品不存在";
                return false;
            }
            if (offer.IsSold)
            {
                message = "该商品已售出";
                return false;
            }
            if (_sessionData.CurrentCoinCount < offer.Price)
            {
                message = "金币不足";
                return false;
            }

            if (offer.IsPotion)
            {
                _sessionData.CurrentCoinCount -= offer.Price;
                float healed = HealByPercent(offer.HealPercent);
                offer.IsSold = true;
                message = healed > 0f ? $"购买成功，恢复 {healed:F1} 生命" : "购买成功，当前生命已满";
                return true;
            }

            if (string.IsNullOrWhiteSpace(offer.ItemId))
            {
                message = "道具数据异常";
                return false;
            }
            if (_sessionData.OwnedItemIds.Contains(offer.ItemId))
            {
                offer.IsSold = true;
                message = "你已拥有该道具";
                return true;
            }

            var itemConfig = _resources.GetConfig<ItemConfig>();
            if (itemConfig == null || !itemConfig.TryGet(offer.ItemId, out var itemData))
            {
                message = "道具配置缺失";
                return false;
            }

            _sessionData.CurrentCoinCount -= offer.Price;
            ApplyBuffIdField(itemData.buffId);
            _sessionData.OwnedItemIds.Add(offer.ItemId);
            offer.IsSold = true;
            message = $"购买成功：{itemData.itemName}";
            return true;
        }

        private bool CanAppearInShop(ItemData item)
        {
            if (_sessionData.OwnedItemIds.Contains(item.itemId)) return false;
            if (item.weight <= 0f) return false;

            string pre = item.prerequisiteItemId;
            return string.IsNullOrWhiteSpace(pre) || pre == "None" || _sessionData.OwnedItemIds.Contains(pre);
        }

        private void AddPotionOffer(int chapter)
        {
            _currentOffers.Add(new ShopOffer
            {
                OfferId = "potion_hp",
                IsPotion = true,
                ItemId = null,
                DisplayName = "活力药剂",
                Description = "立即恢复 35% 最大生命值",
                IconPath = null,
                Price = Mathf.Max(10, 18 + chapter * 6),
                IsSold = false,
                HealPercent = 0.35f
            });
        }

        private static int CalculateItemPrice(ItemRarity rarity, int chapter)
        {
            int basePrice = rarity switch
            {
                ItemRarity.White => 16,
                ItemRarity.Rare => 28,
                ItemRarity.Epic => 44,
                ItemRarity.Legendary => 68,
                _ => 20
            };
            float chapterScale = 1f + (chapter - 1) * 0.2f;
            return Mathf.RoundToInt(basePrice * chapterScale);
        }

        private float HealByPercent(float percent)
        {
            var stats = _playerModel?.Stats;
            if (stats == null || percent <= 0f) return 0f;

            float maxHp = stats.GetStat(StatType.Health)?.FinalValue ?? 0f;
            if (maxHp <= 0f) return 0f;

            float curHp = stats.GetCurrentStatValue(StatType.Health);
            float healAmount = maxHp * percent;
            float targetHp = Mathf.Min(maxHp, curHp + healAmount);
            float actualHeal = targetHp - curHp;
            if (actualHeal <= 0f) return 0f;

            stats.SetCurrentStatValue(StatType.Health, targetHp);
            return actualHeal;
        }

        private void ApplyBuffIdField(string buffIdField)
        {
            if (string.IsNullOrWhiteSpace(buffIdField)) return;

            foreach (var id in buffIdField.Split('|'))
            {
                var buffId = id.Trim();
                if (!string.IsNullOrWhiteSpace(buffId))
                    _buffSystem.AddBuff(buffId);
            }
        }

        private static List<ItemData> WeightedSampleWithoutReplacement(List<ItemData> pool, int count)
        {
            var result = new List<ItemData>(count);
            var remaining = new List<ItemData>(pool);

            while (result.Count < count && remaining.Count > 0)
            {
                float total = remaining.Sum(i => i.weight);
                if (total <= 0f) break;

                float random = Random.Range(0f, total);
                float cumulative = 0f;

                for (int i = 0; i < remaining.Count; i++)
                {
                    cumulative += remaining[i].weight;
                    if (random <= cumulative)
                    {
                        result.Add(remaining[i]);
                        remaining.RemoveAt(i);
                        break;
                    }
                }
            }

            return result;
        }
    }
}

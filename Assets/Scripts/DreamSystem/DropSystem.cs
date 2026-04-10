using System.Collections.Generic;
using System.Linq;
using Data;
using DreamConfig;
using DreamManager;
using Enum.Item;
using Struct;
using UnityEngine;

namespace DreamSystem
{
    public class DropSystem
    {
        private readonly ResourcesManager _resourcesManager;
        private readonly GameSessionData _sessionData;

        public DropSystem(ResourcesManager resourcesManager, GameSessionData sessionData)
        {
            _resourcesManager = resourcesManager;
            _sessionData = sessionData;
        }

        /// <summary>
        /// 根据掉落组 ID 和基础金币量，随机生成本关的掉落奖励（金币 + 1~3 件道具）。
        /// </summary>
        /// <param name="dropGroupId">掉落组 ID（决定稀有度范围）</param>
        /// <param name="baseGold">基础金币数（会有 ±25% 浮动）</param>
        public List<ItemDropResult> RollStageDrop(string dropGroupId, int baseGold)
        {
            var results = new List<ItemDropResult>();

            // ── 金币 ─────────────────────────────────────────────────────────
            int variance  = Mathf.Max(1, baseGold / 4);
            int goldAmount = Mathf.Max(1, baseGold + Random.Range(-variance, variance + 1));
            results.Add(new ItemDropResult { IsGold = true, GoldAmount = goldAmount, IsSelected = true });

            // ── 道具 ─────────────────────────────────────────────────────────
            var itemConfig = _resourcesManager.GetConfig<ItemConfig>();
            if (itemConfig == null) return results;

            var pool = BuildPool(dropGroupId, itemConfig, _sessionData.OwnedItemIds);
            if (pool.Count == 0) return results;

            int numDrops   = Random.Range(1, Mathf.Min(4, pool.Count + 1));   // 1~3 件
            var usedIds    = new HashSet<string>();

            for (int i = 0; i < numDrops && usedIds.Count < pool.Count; i++)
            {
                var available = pool.Where(d => !usedIds.Contains(d.itemId)).ToList();
                if (available.Count == 0) break;

                var picked = WeightedPick(available);
                results.Add(new ItemDropResult
                {
                    IsGold     = false,
                    ItemId     = picked.itemId,
                    Count      = 1,
                    // 道具默认不选中，必须玩家主动点击后才会在 Confirm 时生效
                    IsSelected = false
                });
                usedIds.Add(picked.itemId);
            }

            return results;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 私有工具
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 根据 dropGroupId 从 ItemConfig 中筛选道具池（按稀有度），并排除已拥有的道具。
        /// </summary>
        private static List<ItemData> BuildPool(string dropGroupId, ItemConfig config, IReadOnlyList<string> ownedIds)
        {
            List<ItemData> pool = dropGroupId switch
            {
                "chest_common"    => config.GetByRarities(ItemRarity.White, ItemRarity.Rare),
                "chest_rare"      => config.GetByRarities(ItemRarity.Rare),
                "chest_elite"     => config.GetByRarities(ItemRarity.Rare, ItemRarity.Epic),
                "chest_epic"      => config.GetByRarities(ItemRarity.Epic),
                "chest_boss_prep" => config.GetByRarities(ItemRarity.Epic, ItemRarity.Legendary),
                "chest_boss"      => config.GetByRarities(ItemRarity.Legendary),
                _                 => config.GetByRarities(ItemRarity.White, ItemRarity.Rare)
            };

            // chest_boss 若无 Legendary 道具则回退到 Epic
            if (pool.Count == 0)
                pool = config.GetByRarities(ItemRarity.Epic);

            // 过滤已拥有的道具（每件道具只能拥有一次）
            if (ownedIds != null && ownedIds.Count > 0)
                pool = pool.Where(d => !ownedIds.Contains(d.itemId)).ToList();

            return pool;
        }

        /// <summary>按 weight 加权随机选取一条。</summary>
        private static ItemData WeightedPick(List<ItemData> pool)
        {
            float total    = pool.Sum(d => d.weight);
            float roll     = Random.Range(0f, total);
            float cursor   = 0f;
            foreach (var item in pool)
            {
                cursor += item.weight;
                if (roll <= cursor) return item;
            }
            return pool[pool.Count - 1];
        }
    }
}

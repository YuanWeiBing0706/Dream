using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Data;
using DreamConfig;
using DreamManager;
using DreamSystem.Damage;
using Struct;
using UnityEngine;
using VContainer;

namespace DreamSystem.UI.ViewModel
{
    /// <summary>
    /// 道具选择面板 ViewModel。
    /// <para>展示小关结算掉落的金币和道具，玩家勾选后确认领取。</para>
    /// </summary>
    public class ItemSelectViewModel : ViewModelBase
    {
        private readonly GameSessionData _sessionData;
        private readonly ResourcesManager _resources;
        private readonly BuffSystem _buffSystem;

        public ResourcesManager Resources => _resources;

        /// 本次掉落的所有奖励条目
        public List<ItemDropResult> Rewards { get; private set; } = new();

        /// 金币奖励总量
        public int GoldReward { get; private set; }

        private UniTaskCompletionSource<bool> _confirmTcs;

        [Inject]
        public ItemSelectViewModel(GameSessionData sessionData, ResourcesManager resources, BuffSystem buffSystem)
        {
            _sessionData = sessionData;
            _resources = resources;
            _buffSystem = buffSystem;
        }

        /// <summary>
        /// 设置本关掉落奖励（由 LevelManager 在结算时调用）。
        /// </summary>
        public void SetRewards(List<ItemDropResult> rewards)
        {
            Rewards = rewards;
            GoldReward = 0;
            foreach (var r in rewards)
            {
                if (r.IsGold) GoldReward += r.GoldAmount;
            }
            NotifyRefresh();
        }

        /// <summary>
        /// 等待玩家确认选择，由 LevelManager 在结算流程中 await。
        /// </summary>
        public UniTask WaitForConfirmation()
        {
            _confirmTcs = new UniTaskCompletionSource<bool>();
            return _confirmTcs.Task;
        }

        /// <summary>
        /// 切换某条道具的勾选状态（金币条目不可取消）。
        /// </summary>
        public void ToggleItem(int index)
        {
            if (index < 0 || index >= Rewards.Count) return;
            if (Rewards[index].IsGold) return;

            Rewards[index].IsSelected = !Rewards[index].IsSelected;
            NotifyRefresh();
        }

        /// <summary>
        /// 玩家点击【确认】后调用：领取金币 + 应用已勾选道具的 Buff。
        /// 关闭 UI 由 View 层通过 Close() 完成。
        /// </summary>
        public void Confirm()
        {
            _sessionData.AddCoin(GoldReward);
            
            int selectedItemCount = 0;
            foreach (var reward in Rewards)
            {
                if (!reward.IsGold && reward.IsSelected) selectedItemCount++;
            }
            UnityEngine.Debug.Log($"[ItemSelect] Confirm 前状态: rewards={Rewards.Count}, selectedItems={selectedItemCount}");

            var itemConfig = _resources.GetConfig<ItemConfig>();
            foreach (var reward in Rewards)
            {
                if (reward.IsGold || !reward.IsSelected) continue;

                // 防止重复获取
                if (_sessionData.OwnedItemIds.Contains(reward.ItemId))
                {
                    UnityEngine.Debug.LogWarning($"[ItemSelect] 道具 {reward.ItemId} 已拥有，跳过。");
                    continue;
                }

                if (itemConfig != null && itemConfig.TryGet(reward.ItemId, out var itemData))
                {
                    if (!string.IsNullOrEmpty(itemData.buffId))
                    {
                        UnityEngine.Debug.Log($"[ItemSelect] 应用道具Buff: itemId={reward.ItemId}, buffIdField={itemData.buffId}");
                        // 支持 | 分隔的多 buffId
                        foreach (var bid in itemData.buffId.Split('|'))
                        {
                            var trimmed = bid.Trim();
                            if (!string.IsNullOrEmpty(trimmed))
                            {
                                UnityEngine.Debug.Log($"[ItemSelect] -> AddBuff({trimmed})");
                                _buffSystem.AddBuff(trimmed);
                            }
                        }
                    }
                    _sessionData.OwnedItemIds.Add(reward.ItemId);
                    UnityEngine.Debug.Log($"[ItemSelect] 领取道具: {itemData.itemName} (buffId={itemData.buffId})");
                }
            }

            UnityEngine.Debug.Log($"[ItemSelect] 领取金币: {GoldReward}，当前总计: {_sessionData.CurrentCoinCount}");
            _confirmTcs?.TrySetResult(true);
        }
    }
}

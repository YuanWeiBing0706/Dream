using System.Collections.Generic;
using DreamConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DreamSystem.UI.ViewModel;

namespace DreamSystem.UI.View
{
    /// <summary>
    /// 道具选择弹窗视图（挂在 Item_Panel_Prefab 根节点）。
    /// <para>
    /// 预制体层级：Item_Panel_Prefab > BackgroundImage > GoldRewardText, ItemCardsContainer, ConfirmButton<br/>
    /// 卡片预制体：ItemCardNode（根节点 Image + Button）<br/>
    /// 悬浮提示：ItemInfo（含 ItemName / ItemMessage 文本）
    /// </para>
    /// </summary>
    public sealed class ItemSelectView : UIView<ItemSelectViewModel>
    {
        [Header("面板元素（拖 BackgroundImage 下的子物体）")]
        [SerializeField] private Transform ItemCardsContainer;
        [SerializeField] private TMP_Text GoldRewardText;
        [SerializeField] private Button ConfirmButton;

        [Header("预制体引用")]
        [SerializeField] private GameObject ItemCardNodePrefab;
        [SerializeField] private GameObject ItemInfoPrefab;

        private readonly List<GameObject> _cards = new();
        private ItemInfoDisplay _itemInfo;

        // ───────────────────────────────────────────────
        // 生命周期
        // ───────────────────────────────────────────────

        protected override void OnBindTyped(ItemSelectViewModel viewModel)
        {
            EnsureItemInfoInstance();

            if (ConfirmButton != null)
            {
                ConfirmButton.onClick.RemoveAllListeners();
                ConfirmButton.onClick.AddListener(OnConfirm);
            }

            RefreshDisplay();
        }

        protected override void OnViewModelRefreshRequested(int refreshKey)
        {
            RefreshDisplay();
        }

        private void OnDestroy()
        {
            if (_itemInfo != null)
                Destroy(_itemInfo.gameObject);
        }

        // ───────────────────────────────────────────────
        // ItemInfo 悬浮卡
        // ───────────────────────────────────────────────

        /// <summary>
        /// 在 Canvas 根节点下创建唯一的 ItemInfo 实例，保证渲染在所有面板之上。
        /// </summary>
        private void EnsureItemInfoInstance()
        {
            if (_itemInfo != null) return;
            if (ItemInfoPrefab == null)
            {
                UnityEngine.Debug.LogWarning("[ItemSelectView] 未指定 ItemInfoPrefab，悬浮提示功能不可用。");
                return;
            }

            // 挂在 Canvas 根节点，确保 z-order 最高
            var canvas = GetComponentInParent<Canvas>();
            Transform parent = canvas != null ? canvas.transform : transform;

            var infoGo = Instantiate(ItemInfoPrefab, parent);
            infoGo.transform.SetAsLastSibling();
            _itemInfo = infoGo.GetComponent<ItemInfoDisplay>();

            if (_itemInfo == null)
            {
                UnityEngine.Debug.LogWarning("[ItemSelectView] ItemInfoPrefab 上缺少 ItemInfoDisplay 组件。");
            }
        }

        private void ShowItemInfo(string name, string desc)
        {
            _itemInfo?.Show(name, desc);
        }

        private void HideItemInfo()
        {
            _itemInfo?.Hide();
        }

        // ───────────────────────────────────────────────
        // 卡片刷新
        // ───────────────────────────────────────────────

        private void RefreshDisplay()
        {
            if (Vm == null) return;

            ClearCards();

            if (GoldRewardText != null)
                GoldRewardText.text = $"× {Vm.GoldReward}";

            if (ItemCardsContainer == null || ItemCardNodePrefab == null) return;

            var itemConfig = Vm.Resources.GetConfig<ItemConfig>();

            for (int i = 0; i < Vm.Rewards.Count; i++)
            {
                var reward = Vm.Rewards[i];
                if (reward.IsGold) continue;

                string itemName = reward.ItemId;
                string itemDesc = string.Empty;
                if (itemConfig != null && itemConfig.TryGet(reward.ItemId, out var data))
                {
                    itemName = data.itemName;
                    itemDesc = data.description;
                }

                // 图标加载：优先使用配置 iconPath，缺失时回退 itemId（你的资源名即 ItemId）
                Sprite iconSprite = null;
                if (itemConfig != null && itemConfig.TryGet(reward.ItemId, out var iconData))
                {
                    var iconPath = string.IsNullOrWhiteSpace(iconData.iconPath)
                        ? iconData.itemId
                        : iconData.iconPath;
                    if (!string.IsNullOrWhiteSpace(iconPath))
                        iconSprite = Vm.Resources.LoadAsset<Sprite>(iconPath);
                }

                var cardObj = Instantiate(ItemCardNodePrefab, ItemCardsContainer);
                var card = cardObj.GetComponent<ItemCardNode>();
                if (card != null)
                {
                    int capturedIndex = i;
                    card.Setup(
                        index: capturedIndex,
                        itemName: itemName,
                        itemDesc: itemDesc,
                        defaultSelected: reward.IsSelected,
                        onToggle: OnItemToggled,
                        onHoverEnter: ShowItemInfo,
                        onHoverExit: HideItemInfo,
                        iconSprite: iconSprite);
                }

                _cards.Add(cardObj);
            }
        }

        // ───────────────────────────────────────────────
        // 事件回调
        // ───────────────────────────────────────────────

        private void OnItemToggled(int index)
        {
            Vm?.ToggleItem(index);
        }

        private void OnConfirm()
        {
            HideItemInfo();
            Vm?.Confirm();
            ClearCards();
            Close();
        }

        private void ClearCards()
        {
            foreach (var card in _cards)
                if (card != null) Destroy(card);
            _cards.Clear();
        }
    }
}

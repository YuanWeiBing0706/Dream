using System.Collections.Generic;
using DreamConfig;
using DreamSystem.UI.ViewModel;
using Struct;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DreamSystem.UI.View
{
    /// <summary>
    /// 商店弹窗：单容器 ItemCardsContainer + ItemShopInfo 预制体（内含 ItemIcon、ItemPrice）。
    /// </summary>
    public sealed class ShopSelectView : UIView<ShopViewModel>
    {
        [Header("布局")]
        [SerializeField]
        private Transform ItemCardsContainer;

        [Header("按钮与金币")]
        [SerializeField]
        private Button ConfirmBuyButton;
        [SerializeField]
        private Button LeaveButton;
        [Tooltip("当前金币文案（例如 CurrentCoins 下的 Text (TMP)）")]
        [SerializeField]
        private TMP_Text GoldText;

        [Header("预制体")]
        [Tooltip("ItemShopInfo：根节点挂 ShopCardItem + Button，子节点 ItemIcon(Image)、ItemPrice(TMP)")]
        [SerializeField]
        private GameObject ItemShopInfoPrefab;
        [SerializeField]
        private GameObject ItemInfoPrefab;

        private readonly List<GameObject> _itemCells = new();
        private ItemInfoDisplay _itemInfo;

        protected override void OnBindTyped(ShopViewModel viewModel)
        {
            EnsureItemInfoInstance();

            if (ConfirmBuyButton != null)
            {
                ConfirmBuyButton.onClick.RemoveAllListeners();
                ConfirmBuyButton.onClick.AddListener(OnConfirmBuyClicked);
            }

            if (LeaveButton != null)
            {
                LeaveButton.onClick.RemoveAllListeners();
                LeaveButton.onClick.AddListener(OnLeaveClicked);
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

        private void EnsureItemInfoInstance()
        {
            if (_itemInfo != null || ItemInfoPrefab == null) return;

            var canvas = GetComponentInParent<Canvas>();
            Transform parent = canvas != null ? canvas.transform : transform;
            var infoGo = Instantiate(ItemInfoPrefab, parent);
            infoGo.transform.SetAsLastSibling();
            _itemInfo = infoGo.GetComponent<ItemInfoDisplay>();
        }
        
        
        private void RefreshDisplay()
        {
            if (Vm == null) return;

            ClearItemCells();

            if (GoldText != null)
                GoldText.text = $"{Vm.CurrentGold}";

            if (ConfirmBuyButton != null)
                ConfirmBuyButton.interactable = Vm.CanConfirmPurchase();

            if (ItemCardsContainer == null || ItemShopInfoPrefab == null) return;

            var itemConfig = Vm.Resources.GetConfig<ItemConfig>();

            for (int i = 0; i < Vm.Offers.Count; i++)
            {
                var offer = Vm.Offers[i];
                string title = offer.DisplayName;
                string desc = offer.Description;
                Sprite iconSprite = null;

                if (!offer.IsPotion && itemConfig != null && itemConfig.TryGet(offer.ItemId, out var itemData))
                {
                    title = string.IsNullOrWhiteSpace(itemData.itemName) ? title : itemData.itemName;
                    desc = string.IsNullOrWhiteSpace(itemData.description) ? desc : itemData.description;
                }

                if (!string.IsNullOrWhiteSpace(offer.IconPath))
                    iconSprite = Vm.Resources.LoadAsset<Sprite>(offer.IconPath);

                var cell = Instantiate(ItemShopInfoPrefab, ItemCardsContainer);
                var card = cell.GetComponent<ShopItemCardNode>();
                if (card == null)
                    UnityEngine.Debug.LogError("[ShopSelectView] ItemShopInfoPrefab 根节点缺少 ShopCardItem 组件。");
                else
                    card.Setup(offer, Vm.SelectedOfferId == offer.OfferId, iconSprite, title, desc, OnOfferClicked, OnHoverEnter, OnHoverExit);

                _itemCells.Add(cell);
            }
        }

        private void OnOfferClicked(ShopOffer offer)
        {
            if (Vm == null || offer == null) return;
            Vm.SelectOffer(offer.OfferId);
        }

        private void OnConfirmBuyClicked()
        {
            Vm?.ConfirmPurchaseSelected();
        }

        private void OnLeaveClicked()
        {
            OnHoverExit();
            Vm?.ConfirmLeaveShop();
            Close();
        }

        private void OnHoverEnter(string name, string desc)
        {
            _itemInfo?.Show(name, desc);
        }

        private void OnHoverExit()
        {
            _itemInfo?.Hide();
        }

        private void ClearItemCells()
        {
            foreach (var go in _itemCells)
                if (go != null)
                    Destroy(go);
            _itemCells.Clear();
        }
    }
}
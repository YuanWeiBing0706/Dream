using System.Collections.Generic;
using UnityEngine;
using DreamSystem.UI.ViewModel;

namespace DreamSystem.UI.View
{
    /// <summary>
    /// 海克斯三选一弹窗视图。
    /// </summary>
    public sealed class HexSelectView : UIView<HexSelectViewModel>
    {
        [Header("Hierarchy References")]
        [SerializeField] private GameObject Dim_Overlay;
        [SerializeField] private GameObject Blue_Container;
        [SerializeField] private Transform Cards_Layout_Group;

        [Header("Prefabs")]
        [SerializeField] private GameObject Hex_Card_Prefab;

        private readonly List<GameObject> _instantiatedCards = new();

        protected override void OnBindTyped(HexSelectViewModel viewModel)
        {
            RefreshDisplay();
        }

        protected override void OnViewModelRefreshRequested(int refreshKey)
        {
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (Vm == null || Cards_Layout_Group == null || Hex_Card_Prefab == null) return;

            ClearCards();

            foreach (var hexData in Vm.Options)
            {
                var cardObj = Instantiate(Hex_Card_Prefab, Cards_Layout_Group);
                var cardItem = cardObj.GetComponent<HexCardItem>();

                if (cardItem != null)
                {
                    cardItem.Setup(hexData, Vm.Resources, OnCardSelected);
                }

                _instantiatedCards.Add(cardObj);
            }

            if (Dim_Overlay != null) Dim_Overlay.SetActive(true);
            if (Blue_Container != null) Blue_Container.SetActive(true);
        }

        private void OnCardSelected(string selectedId)
        {
            // 1. 通知 ViewModel 应用 Hex 效果并解锁流程
            Vm.SelectHex(selectedId);

            // 2. 清理卡牌
            ClearCards();

            // 3. 通过 UIViewBase.Close() 走完整生命周期：
            //    OnClose() → UnbindViewModel() (触发 ViewModel.OnExit()) → RequestClose() → UIManager.CloseWindow()
            Close();
        }

        private void ClearCards()
        {
            foreach (var card in _instantiatedCards)
            {
                if (card != null) Destroy(card);
            }
            _instantiatedCards.Clear();
        }
    }
}

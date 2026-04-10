using DreamSystem.UI.ViewModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DreamSystem.UI.View
{
    /// <summary>
    /// 游戏结束窗口（死亡/通关统一使用）。
    /// </summary>
    public sealed class GameResultView : UIView<GameResultViewModel>
    {
        [SerializeField] private Button ReturnLobbyButton;

        protected override void OnBindTyped(GameResultViewModel viewModel)
        {
            if (ReturnLobbyButton != null)
            {
                ReturnLobbyButton.onClick.RemoveAllListeners();
                ReturnLobbyButton.onClick.AddListener(OnReturnLobbyClicked);
            }

            RefreshDisplay();
        }

        protected override void OnViewModelRefreshRequested(int refreshKey)
        {
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (Vm == null) return;
        }

        private void OnReturnLobbyClicked()
        {
            Vm?.ConfirmReturnLobby();
            Close();
        }
    }
}

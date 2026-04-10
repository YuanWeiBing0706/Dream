using DreamSystem.UI.ViewModel;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DreamSystem.UI.View
{
    public class LobbyMainView : UIView<LobbyViewModel>
    {
        [Header("交互按钮")]
        [SerializeField] private Button StartGameBtn;
        [SerializeField] private Button SettingsBtu;
        [SerializeField] private Button ExitGameBtu;


        protected override void OnBindTyped(LobbyViewModel viewModel)
        {
            if (StartGameBtn != null)
            {
                StartGameBtn.onClick.AddListener(OnStartGameClicked);

                // 确保界面打开时按钮是可点的
                StartGameBtn.interactable = true;
            }

            if (SettingsBtu != null)
            {
                SettingsBtu.onClick.AddListener(OnSettingsClicked);
                SettingsBtu.interactable = true;
            }

            if (ExitGameBtu != null)
            {
                ExitGameBtu.onClick.AddListener(OnExitGameClicked);
                ExitGameBtu.interactable = true;
            }
        }

        protected override void OnUnbindTyped(LobbyViewModel viewModel)
        {
            if (StartGameBtn != null)
            {
                StartGameBtn.onClick.RemoveListener(OnStartGameClicked);
            }

            if (SettingsBtu != null)
            {
                SettingsBtu.onClick.RemoveListener(OnSettingsClicked);
            }

            if (ExitGameBtu != null)
            {
                ExitGameBtu.onClick.RemoveListener(OnExitGameClicked);
            }
        }

        private async void OnStartGameClicked()
        {
            // 1. 防连击：立刻禁用按钮
            if (StartGameBtn != null) StartGameBtn.interactable = false;

            // 2. 这里的 VM 请求现在是异步的，会一直等到选角界面关闭
            await this.Vm.RequestOpenCharacterSelect();

            // 3. 选角界面关闭后（且没切场景的情况下），恢复按钮点击
            if (StartGameBtn != null)
            {
                StartGameBtn.interactable = true;
            }
        }


        private void OnSettingsClicked()
        {
            UnityEngine.Debug.Log("OnSettingsClicked");
        }


        private void OnExitGameClicked()
        {
            this.Vm?.RequestExitGame();
        }
    }
}

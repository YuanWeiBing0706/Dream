using Const;
using Cysharp.Threading.Tasks;
using DreamManager;
using DreamSystem.UI.ViewModel;
using VContainer;
using VContainer.Unity;
using UnityEngine;

namespace DreamSystem.UI
{
    /// <summary>
    /// 大厅场景的启动入口，负责一进大厅就自动拉起主 UI 面板
    /// </summary>
    public class LobbyBootSystem : IStartable
    {
        private readonly UIManager _uiManager;
        private readonly LobbyViewModel _lobbyVm;

        [Inject]
        public LobbyBootSystem(UIManager uiManager, LobbyViewModel lobbyVm)
        {
            _uiManager = uiManager;
            _lobbyVm = lobbyVm;
        }

        public void Start()
        {
            // 确保大厅中鼠标是可见且自由的
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 一进入场景，自动打开大厅全屏主界面
            _uiManager.ShowViewAsync(UIPanelIds.LOBBY_MAIN_VIEW, _lobbyVm).Forget();
        }
    }
}

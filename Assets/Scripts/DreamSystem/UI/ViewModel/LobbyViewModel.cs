using Const;
using Cysharp.Threading.Tasks;
using Data;
using DreamManager;
using VContainer;

namespace DreamSystem.UI.ViewModel
{
    public class LobbyViewModel : ViewModelBase
    {
        private readonly UIManager _uiManager;
        private readonly CharacterSelectViewModel _charSelectVm;

        // VContainer 注入大管家以及选角面板的数据模型
        [Inject]
        public LobbyViewModel(UIManager uiManager, CharacterSelectViewModel charSelectVm)
        {
            _uiManager = uiManager;
            _charSelectVm = charSelectVm;
        }

        /// <summary>
        /// 当大厅面板上的“开始游戏”被点击时触发，返回异步任务供 UI 监听
        /// </summary>
        public async UniTask RequestOpenCharacterSelect()
        {
            // 1. 重置并获取选角界面的异步信号
            var selectionTask = _charSelectVm.OpenAsync();

            // 2. 打开弹窗
            _uiManager.ShowWindowAsync(UIPanelIds.CHARACTER_SELECT_VIEW, _charSelectVm).Forget();

            // 3. 阻塞等待：直到玩家在选角界面点了“确定”或“取消”
            await selectionTask;
        }
    }
}
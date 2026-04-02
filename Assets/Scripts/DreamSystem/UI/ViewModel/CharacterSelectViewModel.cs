using Const;
using Cysharp.Threading.Tasks;
using Data;
using DreamManager;
using VContainer;

namespace DreamSystem.UI.ViewModel
{
    public class CharacterSelectViewModel : ViewModelBase
    {
        private readonly SceneFlowManager _sceneFlow;
        private readonly GameSessionData _sessionData;
        private readonly UIManager _uiManager;

        private UniTaskCompletionSource _uniTaskCompletionSource;

        [Inject]
        public CharacterSelectViewModel(SceneFlowManager sceneFlow, GameSessionData sessionData, UIManager uiManager)
        {
            _sceneFlow = sceneFlow;
            _sessionData = sessionData;
            _uiManager = uiManager;
        }

        /// <summary>
        /// 开启选角逻辑，重置任务信号
        /// </summary>
        public UniTask OpenAsync()
        {
            _uniTaskCompletionSource = new UniTaskCompletionSource();
            return _uniTaskCompletionSource.Task;
        }

        /// <summary>
        /// 玩家确认选择英雄，进入游戏场地
        /// </summary>
        public void ConfirmCharacterAndEnterGame(string characterId)
        {
            // 把玩家选好的英雄 ID 写入全局持久化数据（跨场景不丢失）
            _sessionData.SelectedCharacterId = characterId;

            UnityEngine.Debug.Log($"[ViewModel] 玩家确认选择了英雄：{characterId}，准备载入战斗场景！");

            // 信号完成
            _uniTaskCompletionSource?.TrySetResult();

            //调用场景流管家，执行切场景（它内部会自动清理所有 UI）
            _sceneFlow.LoadBattleScenes().Forget();
        }

        /// <summary>
        /// 玩家取消选择，关闭本弹窗
        /// </summary>
        public void CancelSelection()
        {
            _uiManager.CloseWindow(UIPanelIds.CHARACTER_SELECT_VIEW);
            _uniTaskCompletionSource?.TrySetResult(); // 同样信号完成，让大厅界面能获得提示并解锁按钮
        }
    }
}

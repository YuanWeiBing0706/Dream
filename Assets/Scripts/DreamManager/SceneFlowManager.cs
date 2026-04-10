using Const;
using Cysharp.Threading.Tasks;
using DreamSystem;
using UnityEngine.SceneManagement;
using VContainer;
namespace DreamManager
{
    public class SceneFlowManager
    {
        private readonly UIManager _uiManager;
        private readonly EventManager _eventManager;
        [Inject]
        public SceneFlowManager(UIManager uiManager, EventManager eventManager)
        {
            _uiManager = uiManager;
            _eventManager = eventManager;
        }

        public async UniTask LoadLobbyScenes()
        {
            _eventManager.Publish(GameEvents.GAME_INPUT_UNLOCKED);
            _uiManager.CloseCurrentView();
            _uiManager.CloseAllWindows();
            await SceneManager.LoadSceneAsync("LobbyScene").ToUniTask();
        }


        public async UniTask LoadBattleScenes()
        {
            _eventManager.Publish(GameEvents.GAME_INPUT_UNLOCKED);
            _uiManager.CloseCurrentView();
            _uiManager.CloseAllWindows();
            await SceneManager.LoadSceneAsync("BattleScene").ToUniTask();
        }
    }
}
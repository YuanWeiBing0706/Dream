using Cysharp.Threading.Tasks;
using DreamSystem;
using UnityEngine.SceneManagement;
using VContainer;
namespace DreamManager
{
    public class SceneFlowManager
    {
        private readonly UIManager _uiManager;

        [Inject]
        public SceneFlowManager(UIManager uiManager)
        {
            _uiManager = uiManager;
        }

        public async UniTask LoadLobbyScenes()
        {
            _uiManager.CloseCurrentView();
            _uiManager.CloseAllWindows();
            await SceneManager.LoadSceneAsync("LobbyScene").ToUniTask();
        }


        public async UniTask LoadBattleScenes()
        {
            _uiManager.CloseCurrentView();
            _uiManager.CloseAllWindows();
            await SceneManager.LoadSceneAsync("BattleScene").ToUniTask();
        }
    }
}
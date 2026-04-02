using Cysharp.Threading.Tasks;
using Function.Initialize;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
namespace DreamManager
{
    public class GameManger : IUniTaskStartable
    {
        private readonly SceneFlowManager _sceneFlowManager;

        [Inject]
        public GameManger(SceneFlowManager sceneFlowManager)
        {
            _sceneFlowManager = sceneFlowManager;
        }

        public async UniTask AsyncStart()
        {
            Debug.Log("[GameFlow] 所有前置系统已就绪，请求进入 LobbyScene 场景...");
            await _sceneFlowManager.LoadLobbyScenes();
            Debug.Log("[GameFlow] LobbyScene 场景加载完毕！");
        }
    }
}
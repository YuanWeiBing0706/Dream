using Cysharp.Threading.Tasks;
using Function.Initialize;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace DreamManager
{
    public class GameManger : IUniTaskStartable
    {
        public async UniTask AsyncStart()
        {
            Debug.Log("[GameFlow] 所有前置系统已就绪，请求进入 BattleScene 场景...");

            // 加载场景
            var handle = SceneManager.LoadSceneAsync("BattleScene");
            await handle;
            Debug.Log("[GameFlow] BattleScene 场景加载完毕！");
        }
    }
}
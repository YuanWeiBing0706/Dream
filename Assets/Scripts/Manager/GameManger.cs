using System;
using Cysharp.Threading.Tasks;
using DreamSystem;
using DreamSystem.Debug;
using Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;
namespace Scope
{
    public class GameManger : IStartable
    {
        readonly ResourcesManager _resources;
        readonly PlayerInputSystem _inputSystem;
        readonly DebugConsoleSystem _debugSystem;

        public GameManger(ResourcesManager res, PlayerInputSystem input, DebugConsoleSystem debug)
        {
            _resources = res;
            _inputSystem = input;
            _debugSystem = debug;
        }
        
        public async void Start()
        {
            Debug.Log("流程开始：正在加载资源...");
            
            await _resources.InitializeAsync();

            Debug.Log("资源加载完毕！开始唤醒各个系统...");
            _inputSystem.Activate();
            _debugSystem.Activate(); 
            
            var handle = SceneManager.LoadSceneAsync("Main");
            await handle;

            Debug.Log("进入 Main 场景完成！");
        }
    }
}
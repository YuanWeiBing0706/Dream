using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
namespace Function.Initialize
{
    /// 收集所有继承了“IUniTaskStartable”接口的类，并且因为继承了IStartable，所有会在游戏一开始异步执行收集到的类的AsyncStart方法。
    public class AsyncLifecycleExecutor : IStartable
    {
        readonly IEnumerable<IUniTaskStartable> _asyncSystems;

        // VContainer 会按照注册顺序，把所有 IUniTaskStartable 打包传进来
        public AsyncLifecycleExecutor(IEnumerable<IUniTaskStartable> asyncSystems)
        {
            _asyncSystems = asyncSystems;
        }

        public async void Start()
        {
            Debug.Log(">>> [Executor] 开始异步初始化流程...");

            foreach (var system in _asyncSystems)
            {
                var name = system.GetType().Name;
                Debug.Log($"-> 正在启动: {name} ...");

                // 关键：串行等待！
                await system.AsyncStart();

                Debug.Log($"<- {name} 启动完成");
            }
            Debug.Log(">>> [Executor] 所有系统就绪，游戏正式运行 <<<");
        }
    }
}
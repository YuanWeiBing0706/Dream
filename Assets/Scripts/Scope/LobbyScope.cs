using DreamManager;
using DreamSystem.UI;
using DreamSystem.UI.ViewModel;
using Model.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;
namespace Scope
{
    public class LobbyScope : LifetimeScope
    {
        [SerializeField] UIModel SceneUIModel;
        
        protected override void Configure(IContainerBuilder builder)
        {
            // 只要在场景构建完成的瞬间，把当前的 UIRoot 塞给位于全局作用域（Root）的 UIManager 即可。
            builder.RegisterBuildCallback(resolver =>
            {
                var uiManager = resolver.Resolve<UIManager>();
                uiManager.BindUIRoot(SceneUIModel);
            });

            // 注册大厅需要用到的所有 ViewModel (作为单例，方便大厅内各个UI互通)
            builder.Register<LobbyViewModel>(Lifetime.Singleton);
            builder.Register<CharacterSelectViewModel>(Lifetime.Singleton);

            // 注册场景启动入口（它会自动调用 Start 方法打开第一个 UI）
            builder.RegisterEntryPoint<LobbyBootSystem>();
        }
    }
}
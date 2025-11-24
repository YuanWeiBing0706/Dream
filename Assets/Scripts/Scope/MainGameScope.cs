using Dream;
using DreamSystem.Player;
using Model.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;
namespace Scope
{
    public class MainGameScope : LifetimeScope
    {
        // 1. 引用场景里的物体
        [SerializeField] PlayerModel playerInScene;
        [SerializeField] Camera mainCamera;

        protected override void Configure(IContainerBuilder builder)
        {
            // 2. 注册 View 组件
            builder.RegisterComponent(playerInScene);
            builder.RegisterComponent(mainCamera);

            // 3. 注册只属于这个场景的 System (EntryPoint)
            // 只有在这里，PlayerAngleViewSystem 才能注入上面的 playerInScene
            builder.RegisterEntryPoint<PlayerAngleViewSystem>();
            builder.RegisterEntryPoint<PlayerMoveSystem>();
        }
    }
}
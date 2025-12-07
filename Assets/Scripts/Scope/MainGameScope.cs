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
        [SerializeField]
        PlayerModel playerInScene;

        [SerializeField]
        Camera mainCamera;

        [SerializeField]
        CharacterController playerCharacterController;

        protected override void Configure(IContainerBuilder builder)
        {
            // 2. 注册 View 组件
            builder.RegisterComponent(playerInScene);
            builder.RegisterComponent(mainCamera);
            builder.RegisterComponent(playerCharacterController);
            
            // 3. 注册只属于这个场景的 System (EntryPoint)
            builder.RegisterEntryPoint<PlayerMoveSystem>();
        }
    }
}
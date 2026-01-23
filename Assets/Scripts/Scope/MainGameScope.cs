using Animancer;
using Cinemachine;
using DreamSystem.Camera;
using DreamSystem.Player;
using KinematicCharacterController;
using Model.Player;
using SO;
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
        CinemachineFreeLook cameraFreeLook;

        [SerializeField]
        Transform mianCameraTransform;

        [SerializeField]
        KinematicCharacterMotor kinematicCharacterMotor;

        [SerializeField]
        AnimancerComponent animancer;
        [SerializeField]
        CharacterAnimationSo characterAnimationSo;
        
        protected override void Configure(IContainerBuilder builder)
        {
            // 2. 注册 View 组件
            builder.RegisterComponent(playerInScene);
            builder.RegisterComponent(mainCamera);
            builder.RegisterComponent(cameraFreeLook);
            builder.RegisterComponent(mianCameraTransform);
            builder.RegisterComponent(kinematicCharacterMotor);
            builder.RegisterComponent(animancer);
            builder.RegisterComponent(characterAnimationSo);
            // 3. 注册只属于这个场景的 System (EntryPoint)
            builder.RegisterEntryPoint<KccMoveController>().AsSelf();
            builder.RegisterEntryPoint<PlayerMoveSystem>();
            builder.RegisterEntryPoint<PlayerAnimationSystem>();
            builder.RegisterEntryPoint<CinemachineProportionalZoom>();
        }
    }
}
using Animancer;
using Cinemachine;
using DreamSystem.Camera;
using DreamSystem.Enemy;
using DreamSystem.Player;
using Interface;
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
        [SerializeField] PlayerModel playerInScene;
        [SerializeField] Camera mainCamera;
        [SerializeField] CinemachineFreeLook cameraFreeLook;
        [SerializeField] Transform mianCameraTransform;
        [SerializeField] KinematicCharacterMotor kinematicCharacterMotor;
        [SerializeField] AnimancerComponent animancer;
        [SerializeField] CharacterAnimationSo characterAnimationSo;
        [SerializeField] PlayerHitBox playerHitBox;

        protected override void Configure(IContainerBuilder builder)
        {
            // View 组件
            builder.RegisterComponent(playerInScene);
            builder.RegisterComponent(mainCamera);
            builder.RegisterComponent(cameraFreeLook);
            builder.RegisterComponent(mianCameraTransform);
            builder.RegisterComponent(kinematicCharacterMotor);
            builder.RegisterComponent(animancer);
            builder.RegisterComponent(characterAnimationSo);
            builder.RegisterComponent(playerHitBox);

            // 1. PlayerAttackSystem 实现 IPlayerAttackContext (先注册，无依赖于 StateMachine 构造)
            builder.RegisterEntryPoint<PlayerAttackSystem>().AsSelf().As<IPlayerAttackContext>();

            // 2. PlayerStateMachine (不依赖 IPlayerAttackContext 构造)
            builder.Register<PlayerStateMachine>(Lifetime.Singleton);

            // 3. KccMoveController (注入 PlayerStateMachine + IPlayerAttackContext)
            builder.RegisterEntryPoint<KccMoveController>().AsSelf().As<IPlayerMoveContext>();

            // 其他 System
            builder.RegisterEntryPoint<PlayerMoveSystem>();
            builder.RegisterEntryPoint<PlayerAnimationSystem>();
            builder.RegisterEntryPoint<PlayerCombatSystem>();
            builder.RegisterEntryPoint<PlayerDamageSystem>();
            builder.RegisterEntryPoint<EnemyDamageSystem>();
            builder.RegisterEntryPoint<CinemachineProportionalZoom>();
        }
    }
}
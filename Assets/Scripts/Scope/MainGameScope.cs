using Animancer;
using Cinemachine;
using DreamManager;
using DreamSystem.Camera;
using DreamSystem.Damage;
using DreamSystem.Enemy;
using DreamSystem.Player;
using Interface;
using KinematicCharacterController;
using Model.Enemy;
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
        [SerializeField] PlayerHitBox[] playerHitBoxs;

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
            builder.RegisterInstance(playerHitBoxs);

            // PlayerAttackSystem 实现 IPlayerAttackContext
            builder.RegisterEntryPoint<PlayerAttackSystem>().AsSelf().As<IPlayerAttackContext>();

            builder.Register<PlayerStateMachine>(Lifetime.Singleton);
            builder.Register<CharacterStats>(Lifetime.Singleton);

            builder.RegisterEntryPoint<KccMoveController>().AsSelf().As<IPlayerMoveContext>();

            // 玩家属性
            builder.Register<DreamSystem.Damage.CharacterStats>(Lifetime.Singleton);

            // 其他 System
            builder.RegisterEntryPoint<PlayerMoveSystem>();
            builder.RegisterEntryPoint<PlayerAnimationSystem>();
            builder.RegisterEntryPoint<PlayerCombatSystem>();
            builder.RegisterEntryPoint<PlayerInjuriedSystem>();
            builder.RegisterEntryPoint<EnemyInjuriedSystem>();
            builder.RegisterEntryPoint<CinemachineProportionalZoom>();
        }
    }
}
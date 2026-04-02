using Animancer;
using Cinemachine;
using DreamManager;
using DreamSystem.Camera;
using DreamSystem.Damage;
using DreamSystem.Damage.Stat;
using DreamSystem.Enemy;
using DreamSystem.Player;
using Interface;
using KinematicCharacterController;
using Model.Enemy;
using Model.Player;
using Model.UI;
using Providers;
using SO;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace Scope
{
    public class MainGameScope : LifetimeScope
    {
        [SerializeField] UIModel SceneUIModel;
        [SerializeField] PlayerModel PlayerInScene;
        [SerializeField] Camera MainCamera;
        [SerializeField] CinemachineFreeLook CameraFreeLook;
        [SerializeField] Transform MianCameraTransform;
        [SerializeField] KinematicCharacterMotor KinematicCharacterMotor;
        [SerializeField] AnimancerComponent Animancer;
        [SerializeField] CharacterAnimationSo CharacterAnimationSo;
        [SerializeField] PlayerHitBox[] PlayerHitBoxList;

        protected override void Configure(IContainerBuilder builder)
        {
            // View 组件（PlayerModel 同时作为 IBuffOwner 注册，供 BuffSystem 注入）
            builder.RegisterComponent(PlayerInScene).As<IBuffOwner>();
            builder.RegisterComponent(MainCamera);
            builder.RegisterComponent(CameraFreeLook);
            builder.RegisterComponent(MianCameraTransform);
            builder.RegisterComponent(KinematicCharacterMotor);
            builder.RegisterComponent(Animancer);
            builder.RegisterComponent(CharacterAnimationSo);
            builder.RegisterInstance(PlayerHitBoxList);

            // 场景构建完毕瞬间，动态绑定 UI 画布给全局大管家
            builder.RegisterBuildCallback(resolver =>
            {
                var uiManager = resolver.Resolve<UIManager>();
                uiManager.BindUIRoot(SceneUIModel);
            });

            // PlayerAttackSystem 实现 IPlayerAttackContext
            builder.RegisterEntryPoint<PlayerAttackSystem>().AsSelf().As<IPlayerAttackContext>();

            builder.Register<PlayerStateMachine>(Lifetime.Singleton);

            builder.Register<BuffSystem>(Lifetime.Singleton);
            builder.RegisterEntryPoint<KccMoveController>().AsSelf().As<IPlayerMoveContext>();

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
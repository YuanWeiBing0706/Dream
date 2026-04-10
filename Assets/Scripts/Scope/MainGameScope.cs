using Animancer;
using Cinemachine;
using DreamManager;
using DreamSystem;
using DreamSystem.Camera;
using DreamSystem.Damage;
using DreamSystem.Damage.Stat;
using DreamSystem.Enemy;
using DreamSystem.Player;
using DreamSystem.UI;
using DreamSystem.UI.ViewModel;
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

        [Tooltip("调试属性面板（可选，不填则跳过注册）")]
        [SerializeField] private StatsDebugPanel StatsDebugPanel;

        protected override void Configure(IContainerBuilder builder)
        {
            // View 组件（PlayerModel 同时作为 IBuffOwner 注册，供 BuffSystem 注入）
            builder.RegisterComponent(PlayerInScene).AsSelf().As<IBuffOwner>();
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
        builder.RegisterEntryPoint<BuffManager>().AsSelf();
            builder.Register<PlayerStateMachine>(Lifetime.Singleton);

            builder.Register<BuffSystem>(Lifetime.Singleton);
            builder.RegisterEntryPoint<KccMoveController>().AsSelf().As<IPlayerMoveContext>();

            // 核心关卡管理与刷怪
            builder.RegisterEntryPoint<WaveManager>().AsSelf();
            builder.Register<DropSystem>(Lifetime.Singleton);
            builder.Register<ShopSystem>(Lifetime.Singleton);
            builder.RegisterEntryPoint<LevelManager>().AsSelf();

            // 战斗 UI ViewModel
            builder.Register<PlayerStatusViewModel>(Lifetime.Singleton);
            builder.Register<HexSelectViewModel>(Lifetime.Singleton);
            builder.Register<ItemSelectViewModel>(Lifetime.Singleton);
            builder.Register<ShopViewModel>(Lifetime.Singleton);
            builder.Register<GameResultViewModel>(Lifetime.Singleton);

            // 调试面板（可选）
            if (StatsDebugPanel != null)
                builder.RegisterComponent(StatsDebugPanel).AsSelf();

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

using DreamManager;
using DreamSystem;
using DreamSystem.Damage;
using DreamSystem.Debug;
using Function.Initialize;
using VContainer;
using VContainer.Unity;
namespace Scope
{
    public class GameProjectScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<EventManager>(Lifetime.Singleton);
            builder.Register<GameInputManager>(Lifetime.Singleton);
            builder.Register<SuggestionService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<DamageSystem>().AsSelf();
            builder.Register<ResourcesManager>(Lifetime.Singleton)
                .As<IUniTaskStartable>() // 贴上标签：我是要异步启动的
                .AsSelf();

            builder.RegisterEntryPoint<PlayerInputSystem>().AsSelf();

            // Debug 系统依赖资源，所以排在 Resource 后面
            builder.RegisterEntryPoint<DebugConsoleSystem>()
                .As<IUniTaskStartable>()
                .AsSelf();


            builder.Register<GameManger>(Lifetime.Singleton)
                .As<IUniTaskStartable>()
                .AsSelf();

            builder.RegisterEntryPoint<AsyncLifecycleExecutor>();
        }
    }
}
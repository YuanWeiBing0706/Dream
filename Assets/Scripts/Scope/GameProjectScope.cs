using System;
using Dream;
using DreamSystem;
using DreamSystem.Debug;
using Manager;
using UnityEngine;
using VContainer;
using VContainer.Unity;
namespace Scope
{
    public class GameProjectScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // 
            builder.Register<ResourcesManager>(Lifetime.Singleton);
            builder.Register<EventManager>(Lifetime.Singleton);
            builder.Register<GameInputManager>(Lifetime.Singleton); 
            
            // 业务系统
            builder.RegisterEntryPoint<PlayerInputSystem>().AsSelf();;
            builder.RegisterEntryPoint<DebugConsoleSystem>().AsSelf();;

            // 流程控制器
            builder.RegisterEntryPoint<GameManger>();
        }
    }
}
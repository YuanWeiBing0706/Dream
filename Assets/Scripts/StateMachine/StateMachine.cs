using System;
using System.Collections.Generic;
using Dream;
using DreamManager;
using DreamSystem;
using DreamSystem.Debug;
using Scope;
using UnityEngine;
using VContainer;
using VContainer.Unity;
namespace machine
{
    public class StateMachine
    {
        // public List<State> states = new List<State>();
        // private State _currentState;
        // public State CurrentState => _currentState;
        public void ToState(IContainerBuilder builder) //ID
        {
            builder.Register<EventManager>(Lifetime.Singleton);
            builder.Register<ResourcesManager>(Lifetime.Singleton);
            builder.Register<GameInputManager>(Lifetime.Singleton); 
            
            // 业务系统
            builder.RegisterEntryPoint<PlayerInputSystem>().AsSelf();
            builder.RegisterEntryPoint<DebugConsoleSystem>().AsSelf();
            
        }
        
        // public void Update()
        // {
        //     if (_currentState == null)
        //     {
        //         return;
        //     }
        //
        //     _currentState.OnUpdate();
        // }
    }
}
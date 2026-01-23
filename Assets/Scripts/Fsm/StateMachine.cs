using Fsm.Base;
using UnityEngine;
namespace Fsm
{
    public class StateMachine
    {
        /// 调试日志开关
        public bool ShowDebugLog = true;
        /// 当前正在运行的状态
        public BaseState CurrentState { get; private set; }

        /// 获取当前状态名称（用于调试）
        public string CurrentStateName => CurrentState != null ? CurrentState.GetType().Name : "None";

        /// <summary>
        /// 初始化状态机
        /// </summary>
        /// <param name="startingState">初始状态实例</param>
        public void Initialize(BaseState startingState)
        {
            CurrentState = startingState;
            CurrentState.OnEnter();
        }

        /// <summary>
        /// 切换到新的状态
        /// <para>流程：Exit 旧状态 -> 设置新状态 -> Enter 新状态</para>
        /// </summary>
        /// <param name="newState">要切换到的目标状态</param>
        public void TransitionTo(BaseState newState)
        {
            if (newState == null) return;
            // 已经在该状态就不切了
            if (newState == CurrentState) return;

            // 彩色日志埋点
            if (ShowDebugLog)
            {
                Debug.Log($"<color=cyan>[FSM] {CurrentStateName} >>> {newState.GetType().Name}</color>");
            }

            CurrentState.OnExit();
            CurrentState = newState;
            CurrentState.OnEnter();
        }
    }
}
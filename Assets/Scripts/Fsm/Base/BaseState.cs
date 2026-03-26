using DreamManager;
using DreamSystem.Player;
using Interface;
using UnityEngine;

namespace Fsm.Base
{
    public abstract class BaseState
    {
        /// <summary>
        /// 是否允许从当前状态退出。默认 true，DeadState 等终态应重写为 false。
        /// </summary>
        public virtual bool CanExit => true;

        /// 移动上下文，提供物理引擎和移动参数
        protected readonly IPlayerMoveContext moveContext;

        /// 事件管理器，用于发布动画事件
        protected readonly EventManager eventManager;

        /// 玩家状态机，用于状态切换
        protected readonly PlayerStateMachine playerStateMachine;

        protected BaseState(IPlayerMoveContext moveContext, EventManager eventManager, PlayerStateMachine playerStateMachine)
        {
            this.moveContext = moveContext;
            this.eventManager = eventManager;
            this.playerStateMachine = playerStateMachine;
        }

        /// <summary>
        /// 进入状态时调用。
        /// </summary>
        public virtual void OnEnter()
        {
            // TODO: 子类可重写以实现进入逻辑
        }

        /// <summary>
        /// 退出状态时调用。
        /// </summary>
        public virtual void OnExit()
        {
            // TODO: 子类可重写以实现退出逻辑
        }

        /// <summary>
        /// 每帧更新，由 KccMoveController.LateTick 驱动。
        /// </summary>
        /// <param name="deltaTime">帧间隔时间</param>
        public virtual void OnUpdate(float deltaTime)
        {
            // TODO: 子类可重写以实现帧更新逻辑
        }

        /// <summary>
        /// 更新速度，由 KCC 物理系统调用。
        /// </summary>
        /// <param name="currentVelocity">当前速度 (引用传递)</param>
        /// <param name="deltaTime">帧间隔时间</param>
        public virtual void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            // TODO: 子类可重写以实现速度更新逻辑
        }

        /// <summary>
        /// 更新旋转，由 KCC 物理系统调用。
        /// </summary>
        /// <param name="currentRotation">当前旋转 (引用传递)</param>
        /// <param name="deltaTime">帧间隔时间</param>
        public virtual void OnUpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            // TODO: 子类可重写以实现旋转更新逻辑
        }
    }
}
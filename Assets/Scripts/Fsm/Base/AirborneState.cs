using DreamManager;
using DreamSystem.Player;
using Interface;
using UnityEngine;

namespace Fsm.Base
{
    public abstract class AirborneState : BaseState
    {
        protected AirborneState(IPlayerMoveContext moveContext, EventManager eventManager, PlayerStateMachine playerStateMachine) : base(moveContext, eventManager, playerStateMachine) { }

        /// <summary>
        /// 检测是否落地，是则切换到移动状态。
        /// </summary>
        /// <returns>是否已切换状态</returns>
        protected bool CheckLanding()
        {
            if (moveContext.Motor.GroundingStatus.IsStableOnGround)
            {
                playerStateMachine.TransitionTo(playerStateMachine.MoveState);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检测空中闪避输入，若未使用过则切换到冲刺状态。
        /// </summary>
        /// <returns>是否已切换状态</returns>
        protected bool CheckAirDash()
        {
            if (moveContext.MoveInputs.isDodge && !moveContext.HasUsedAirDash)
            {
                playerStateMachine.TransitionTo(playerStateMachine.DashState);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检测空中跳跃输入，若未使用过则切换到跳跃状态。
        /// </summary>
        /// <returns>是否已切换状态</returns>
        protected bool CheckAirJump()
        {
            if (moveContext.MoveInputs.jumpDown && !moveContext.HasUsedJump)
            {
                playerStateMachine.TransitionTo(playerStateMachine.JumpState);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 应用重力和空气阻力。
        /// </summary>
        /// <param name="currentVelocity">当前速度 (引用传递)</param>
        /// <param name="deltaTime">帧间隔时间</param>
        public override void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            // 应用重力
            currentVelocity += moveContext.Gravity * deltaTime;
            // 应用空气阻力
            currentVelocity *= (1f / (1f + (moveContext.Drag * deltaTime)));
        }
    }
}
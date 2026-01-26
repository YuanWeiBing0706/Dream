using DreamManager;
using DreamSystem.Player;
using Interface;
using UnityEngine;

namespace Fsm.Base
{
    public abstract class GroundedState : BaseState
    {
        protected GroundedState(IPlayerMoveContext moveContext, EventManager eventManager, PlayerStateMachine playerStateMachine)
            : base(moveContext, eventManager, playerStateMachine) { }

        /// <summary>
        /// 进入地面状态时重置空中技能标记。
        /// </summary>
        public override void OnEnter()
        {
            base.OnEnter();
            moveContext.HasUsedAirDash = false;
            moveContext.HasUsedJump = false;
        }

        /// <summary>
        /// 检测是否离开地面，是则切换到下落状态。
        /// </summary>
        /// <returns>是否已切换状态</returns>
        protected bool CheckFalling()
        {
            if (!moveContext.Motor.GroundingStatus.IsStableOnGround)
            {
                playerStateMachine.TransitionTo(playerStateMachine.FallState);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检测跳跃输入，是则切换到跳跃状态。
        /// </summary>
        /// <returns>是否已切换状态</returns>
        protected bool CheckJump()
        {
            if (moveContext.MoveInputs.jumpDown)
            {
                playerStateMachine.TransitionTo(playerStateMachine.JumpState);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检测闪避输入，根据是否锁定切换到翻滚或冲刺状态。
        /// </summary>
        /// <returns>是否已切换状态</returns>
        protected bool CheckDodge()
        {
            if (moveContext.MoveInputs.isDodge)
            {
                if (moveContext.MoveInputs.isLockedOn)
                {
                    playerStateMachine.TransitionTo(playerStateMachine.RollState);
                }
                else
                {
                    playerStateMachine.TransitionTo(playerStateMachine.DashState);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 将速度投影到地面法线方向，保持在斜面上的稳定移动。
        /// </summary>
        /// <param name="currentVelocity">当前速度 (引用传递)</param>
        /// <param name="deltaTime">帧间隔时间</param>
        public override void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            var motor = moveContext.Motor;
            currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;
        }
    }
}
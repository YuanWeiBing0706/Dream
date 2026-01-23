using DreamManager;
using DreamSystem.Player;
using UnityEngine;
namespace Fsm.Base
{
    public abstract class GroundedState : BaseState
    {
        protected GroundedState(KccMoveController kccMoveController,EventManager eventManager) : base(kccMoveController,eventManager) { }

        public override void OnEnter()
        {
            base.OnEnter();
            kccMoveController.hasUsedAirDash = false;
            kccMoveController.hasUsedJump=false;
        }
        
        /// <summary>
        /// 检测是否失去地面稳定性（跌落）
        /// </summary>
        /// <returns>如果发生状态切换返回 true</returns>
        protected bool CheckFalling()
        {
            if (!kccMoveController.kinematicCharacterMotor.GroundingStatus.IsStableOnGround)
            {
                kccMoveController.StateMachine.TransitionTo(kccMoveController.fallState);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检测跳跃输入
        /// </summary>
        /// <returns>如果发生状态切换返回 true</returns>
        protected bool CheckJump()
        {
            if (kccMoveController.currentInputs.jumpDown)
            {
                kccMoveController.StateMachine.TransitionTo(kccMoveController.jumpState);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检测闪避输入（Dash 或 Roll）
        /// </summary>
        /// <returns>如果发生状态切换返回 true</returns>
        protected bool CheckDodge()
        {
            if (kccMoveController.currentInputs.isDodge)
            {
                if (kccMoveController.currentInputs.isLockedOn)
                {
                    kccMoveController.StateMachine.TransitionTo(kccMoveController.rollState);
                }
                else
                {
                    kccMoveController.StateMachine.TransitionTo(kccMoveController.dashState);
                }
                return true;
            }
            return false;
        }
        
        public override void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            // 只要在地上，咱们就得算坡度，不能直接向前飞
            var motor = kccMoveController.kinematicCharacterMotor;
            currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;
        }
        
    }
}
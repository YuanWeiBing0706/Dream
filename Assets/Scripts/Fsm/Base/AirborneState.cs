using DreamManager;
using DreamSystem.Player;
using UnityEngine;
namespace Fsm.Base
{
    public abstract class AirborneState : BaseState
    {
        protected AirborneState(KccMoveController kccMoveController, EventManager eventManager) : base(kccMoveController, eventManager) { }

        /// <summary>
        /// 检测是否落地
        /// </summary>
        /// <returns>如果发生状态切换返回 true</returns>
        protected bool CheckLanding()
        {
            if (kccMoveController.kinematicCharacterMotor.GroundingStatus.IsStableOnGround)
            {
                kccMoveController.StateMachine.TransitionTo(kccMoveController.moveState);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检测空中冲刺输入
        /// </summary>
        /// <returns>如果发生状态切换返回 true</returns>
        protected bool CheckAirDash()
        {
            if (kccMoveController.currentInputs.isDodge && !kccMoveController.hasUsedAirDash)
            {
                kccMoveController.StateMachine.TransitionTo(kccMoveController.dashState);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检测空中二段跳输入
        /// </summary>
        /// <returns>如果发生状态切换返回 true</returns>
        protected bool CheckAirJump()
        {
            if (kccMoveController.currentInputs.jumpDown && !kccMoveController.hasUsedJump)
            {
                kccMoveController.StateMachine.TransitionTo(kccMoveController.jumpState);
                return true;
            }
            return false;
        }

        public override void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            currentVelocity += kccMoveController.gravity * deltaTime;
            currentVelocity *= (1f / (1f + (kccMoveController.drag * deltaTime)));
        }
    }
}
using DreamManager;
using DreamSystem.Player;
using Fsm.Base;
using Interface;
using UnityEngine;

namespace Fsm.State.Grounded
{
    public class MoveState : GroundedState
    {
        public MoveState(IPlayerMoveContext moveContext, EventManager eventManager, PlayerStateMachine playerStateMachine) : base(moveContext, eventManager, playerStateMachine) { }

        /// <summary>
        /// 每帧检测跌落、跳跃、闪避输入。
        /// </summary>
        /// <param name="deltaTime">帧间隔时间</param>
        public override void OnUpdate(float deltaTime)
        {
            if (CheckFalling()) return;
            if (CheckJump()) return;
            if (CheckDodge()) return;
        }

        /// <summary>
        /// 根据输入方向平滑更新移动速度。
        /// </summary>
        /// <param name="currentVelocity">当前速度 (引用传递)</param>
        /// <param name="deltaTime">帧间隔时间</param>
        public override void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            base.OnUpdateVelocity(ref currentVelocity, deltaTime);

            // 计算目标速度
            Vector3 targetVelocity = moveContext.MoveInputs.moveDirection * moveContext.MaxStableMoveSpeed;

            // 平滑过渡到目标速度
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1f - Mathf.Exp(-moveContext.StableMovementSharpness * deltaTime));
        }

        /// <summary>
        /// 根据移动方向平滑更新角色朝向。
        /// </summary>
        /// <param name="currentRotation">当前旋转 (引用传递)</param>
        /// <param name="deltaTime">帧间隔时间</param>
        public override void OnUpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            var inputs = moveContext.MoveInputs;

            if (inputs.moveDirection.sqrMagnitude > 0f)
            {
                // 计算目标朝向
                Vector3 lookDirection = inputs.moveDirection.normalized;
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection, moveContext.Motor.CharacterUp);

                // 平滑旋转
                currentRotation = Quaternion.RotateTowards(currentRotation, targetRotation, moveContext.RotationSpeed * deltaTime);
            }
        }
    }
}
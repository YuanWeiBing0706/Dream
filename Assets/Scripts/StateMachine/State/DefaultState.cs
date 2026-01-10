using DreamSystem.Player;
using StateMachine.Base;
using UnityEngine;
namespace StateMachine.State
{
    public class DefaultState : MovementState
    {
        public DefaultState(KccMoveController kccMoveController) : base(kccMoveController) { }

        /// <summary>
        /// 每帧更新速度向量。
        /// </summary>
        /// <param name="currentVelocity">当前速度（引用）</param>
        /// <param name="deltaTime">时间步长</param>
        public override void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            // 获取数据的快捷引用
            var motor = kccMoveController.kinematicCharacterMotor;
            var inputs = kccMoveController.currentInputs;
            
            if (motor.GroundingStatus.IsStableOnGround)
            {
                float currentVelocityMagnitude = currentVelocity.magnitude;
                Vector3 effectiveGroundNormal = motor.GroundingStatus.GroundNormal;

                // 沿坡度切线方向重定向速度，保证在斜坡上移动不掉速
                currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

                // 计算目标输入速度
                // inputs.MoveDirection 已经在 PlayerMoveSystem 里计算为世界坐标方向了
                Vector3 targetMovementVelocity = inputs.moveDirection * kccMoveController.maxStableMoveSpeed;

                // 地面惯性平滑 (Lerp)
                // stableMovementSharpness 越大，响应越快，惯性越小
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-kccMoveController.stableMovementSharpness * deltaTime));
            }
            else
            {
                // 空中变向（空气动力控制）
                if (inputs.moveDirection.sqrMagnitude > 0f)
                {
                    Vector3 targetMovementVelocity = inputs.moveDirection * kccMoveController.maxAirMoveSpeed;

                    // 保证空中加速时不影响垂直重力，只改变水平分量
                    Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, kccMoveController.gravity);
                    currentVelocity += velocityDiff * kccMoveController.airAccelerationSpeed * deltaTime;
                }

                // 施加重力
                currentVelocity += kccMoveController.gravity * deltaTime;

                // 施加空气阻力
                currentVelocity *= (1f / (1f + (kccMoveController.drag * deltaTime)));
            }
            
            if (inputs.jumpDown)
            {
                // 简单的跳跃判定：在地面上才能跳
                // (如果要加土狼时间，可以去 kccMoveController 里访问计时的变量)
                if (motor.GroundingStatus.IsStableOnGround)
                {
                    // 强制角色离开地面，告诉物理马达“我现在断开地面吸附了”
                    motor.ForceUnground(0.1f);

                    // 施加向上速度
                    // 先减去当前的垂直分量，确保每次跳跃高度一致，不受之前下落速度影响
                    currentVelocity += (motor.CharacterUp * kccMoveController.jumpSpeed) - Vector3.Project(currentVelocity, motor.CharacterUp);
                }
            }
        }

        /// <summary>
        /// 每帧更新旋转朝向。
        /// </summary>
        /// <param name="currentRotation">当前旋转（引用）</param>
        /// <param name="deltaTime">时间步长</param>
        public override void OnUpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            var inputs = kccMoveController.currentInputs;

            // 只有当有移动输入时才改变朝向
            if (inputs.moveDirection != Vector3.zero)
            {
                // 计算目标朝向
                Quaternion targetRotation = Quaternion.LookRotation(inputs.moveDirection, kccMoveController.kinematicCharacterMotor.CharacterUp);
                // 匀速平滑旋转到目标朝向
                currentRotation = Quaternion.RotateTowards(currentRotation, targetRotation, kccMoveController.rotationSpeed * deltaTime);
            }
        }
    }
}
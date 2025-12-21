using DreamSystem.Player;
using StateMachine.Base;
using UnityEngine;
namespace StateMachine.State
{
    public class DefaultState : MovementState
    {
        public DefaultState(KccMoveController kccMoveController) : base(kccMoveController) { }

        public override void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            // 获取数据的快捷引用
            var motor = kccMoveController.kinematicCharacterMotor;
            var inputs = kccMoveController.currentInputs; 

            // ================= 地面逻辑 =================
            if (motor.GroundingStatus.IsStableOnGround)
            {
                float currentVelocityMagnitude = currentVelocity.magnitude;
                Vector3 effectiveGroundNormal = motor.GroundingStatus.GroundNormal;

                // 1. 沿坡度切线方向重定向速度
                currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

                // 2. 计算目标输入速度
                // inputs.MoveDirection 已经在 PlayerMoveSystem 里计算为世界坐标方向了
                Vector3 targetMovementVelocity = inputs.moveDirection * kccMoveController.maxStableMoveSpeed;

                // 3. 地面惯性平滑
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-kccMoveController.stableMovementSharpness * deltaTime));
            }
            // ================= 空中逻辑 =================
            else
            {
                // 1. 空中变向
                if (inputs.moveDirection.sqrMagnitude > 0f)
                {
                    Vector3 targetMovementVelocity = inputs.moveDirection * kccMoveController.maxAirMoveSpeed;
                    
                    // 保证空中加速时不影响垂直重力
                    Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, kccMoveController.gravity);
                    currentVelocity += velocityDiff * kccMoveController.airAccelerationSpeed * deltaTime;
                }

                // 2. 重力
                currentVelocity += kccMoveController.gravity * deltaTime;

                // 3. 空气阻力
                currentVelocity *= (1f / (1f + (kccMoveController.drag * deltaTime)));
            }

            // ================= 跳跃逻辑 =================
            if (inputs.jumpDown)
            {
                // 简单的跳跃判定：在地面上才能跳
                // (如果要加土狼时间，可以去 kccMoveController 里访问计时的变量)
                if (motor.GroundingStatus.IsStableOnGround)
                {
                    // 强制离地
                    motor.ForceUnground(0.1f);
                    
                    // 施加向上速度
                    // 先减去当前的垂直分量，确保每次跳跃高度一致，不受之前下落速度影响
                    currentVelocity += (motor.CharacterUp * kccMoveController.jumpSpeed) - Vector3.Project(currentVelocity, motor.CharacterUp);
                }
            }
        }

        public override void OnUpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            var inputs = kccMoveController.currentInputs;
            
            if (inputs.moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(inputs.moveDirection, kccMoveController.kinematicCharacterMotor.CharacterUp);
                currentRotation = Quaternion.RotateTowards(currentRotation, targetRotation, kccMoveController.rotationSpeed * deltaTime);
            }
        }
    }
}
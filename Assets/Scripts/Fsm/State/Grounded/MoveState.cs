using DreamManager;
using DreamSystem.Player;
using Fsm.Base;
using UnityEngine;
namespace Fsm.State.Grounded
{
    public class MoveState : GroundedState
    {
        public MoveState(KccMoveController kccMoveController, EventManager eventManager) : base(kccMoveController, eventManager) { }

        public override void OnUpdate(float deltaTime)
        {
            // 显式工具方法调用（优先级从高到低）
            if (CheckFalling()) return;  // 优先检测物理跌落
            if (CheckJump()) return;      // 允许跳跃
            if (CheckDodge()) return;     // 允许闪避
        }

        public override void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            base.OnUpdateVelocity(ref currentVelocity, deltaTime);

            // 计算目标速度
            Vector3 targetVelocity = kccMoveController.currentInputs.moveDirection * kccMoveController.maxStableMoveSpeed;
            
            // 平滑插值到目标速度（带惯性）
            currentVelocity = Vector3.Lerp(
                currentVelocity, 
                targetVelocity, 
                1f - Mathf.Exp(-kccMoveController.stableMovementSharpness * deltaTime)
            );
        }
        
        public override void OnUpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            // 1. 获取输入
            var inputs = kccMoveController.currentInputs;

            // 2. 如果有移动输入 (防止松开按键后瞬间转回0度)
            if (inputs.moveDirection.sqrMagnitude > 0f)
            {
                // 3. 计算目标朝向：Z轴朝向移动方向，Y轴朝向头顶
                Vector3 lookDirection = inputs.moveDirection.normalized;
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection, kccMoveController.kinematicCharacterMotor.CharacterUp);

                // 4. 平滑旋转 (RotateTowards)
                // rotationSpeed 应该在 Controller 里定义，比如 720
                currentRotation = Quaternion.RotateTowards(currentRotation, targetRotation, kccMoveController.rotationSpeed * deltaTime);
            }
        }
    }
}
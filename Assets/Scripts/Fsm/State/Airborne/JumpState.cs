using DreamManager;
using DreamSystem.Player;
using Events;
using Fsm.Base;
using UnityEngine;

namespace Fsm.State.Airborne
{
    public class JumpState : AirborneState
    {

        public JumpState(KccMoveController kccMoveController, EventManager eventManager) : base(kccMoveController, eventManager)
        {

        }

        public override void OnEnter()
        {
            // 虽然你可能没写逻辑，但保留调用父类是个好习惯
            base.OnEnter();
            kccMoveController.hasUsedJump = true;
            // 1. 强制离地 (必须有)
            kccMoveController.kinematicCharacterMotor.ForceUnground(0.1f);

            // 2. 获取当前速度
            Vector3 currentVelocity = kccMoveController.kinematicCharacterMotor.BaseVelocity;
            Vector3 jumpDir = kccMoveController.kinematicCharacterMotor.CharacterUp; // 通常是 Vector3.up

            // ProjectOnPlane 意思是：把 currentVel 拍扁在水平面上，去掉 Y 轴分量
            Vector3 planarVel = Vector3.ProjectOnPlane(currentVelocity, jumpDir);

            // 4. 重新赋值：水平速度(保留) + 新的跳跃速度
            kccMoveController.kinematicCharacterMotor.BaseVelocity = planarVel + (jumpDir * kccMoveController.jumpSpeed);

            eventManager.Publish(GameEvents.PLAYER_JUMP_ANIMATION);
        }

        public override void OnUpdate(float deltaTime)
        {
            // 绝不调用 CheckLanding() - 防止起跳瞬间被检测为落地

            // 允许空中冲刺
            if (CheckAirDash()) return;

            // 检测到达顶点 (开始下落)
            float verticalSpeed = kccMoveController.kinematicCharacterMotor.Velocity.y;

            if (verticalSpeed <= 0f)
            {
                kccMoveController.StateMachine.TransitionTo(kccMoveController.fallState);
            }
        }
    }
}
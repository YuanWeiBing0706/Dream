using DreamManager;
using DreamSystem.Player;
using Events;
using Fsm.Base;
using Interface;

namespace Fsm.State.Airborne
{
    public class JumpState : AirborneState
    {
        public JumpState(IPlayerMoveContext moveContext, EventManager eventManager, PlayerStateMachine playerStateMachine) : base(moveContext, eventManager, playerStateMachine) { }

        /// <summary>
        /// 进入跳跃状态：施加跳跃力并播放动画。
        /// </summary>
        public override void OnEnter()
        {
            base.OnEnter();
            moveContext.HasUsedJump = true;

            // 强制脱离地面 (土狼时间支持)
            moveContext.Motor.ForceUnground(0.1f);

            // 计算跳跃速度：保留水平速度，叠加垂直跳跃
            UnityEngine.Vector3 currentVelocity = moveContext.Motor.BaseVelocity;
            UnityEngine.Vector3 jumpDir = moveContext.Motor.CharacterUp;
            UnityEngine.Vector3 planarVel = UnityEngine.Vector3.ProjectOnPlane(currentVelocity, jumpDir);

            moveContext.Motor.BaseVelocity = planarVel + (jumpDir * moveContext.JumpSpeed);

            eventManager.Publish(GameEvents.PLAYER_JUMP_ANIMATION);
        }

        /// <summary>
        /// 每帧检测空中闪避和下落转换。
        /// </summary>
        /// <param name="deltaTime">帧间隔时间</param>
        public override void OnUpdate(float deltaTime)
        {
            if (CheckAirDash()) return;

            // 垂直速度 <= 0 时切换到下落状态
            float verticalSpeed = moveContext.Motor.Velocity.y;
            if (verticalSpeed <= 0f)
            {
                playerStateMachine.TransitionTo(playerStateMachine.FallState);
            }
        }
    }
}
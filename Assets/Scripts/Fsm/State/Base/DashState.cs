using DreamManager;
using DreamSystem.Player;
using Events;
using Fsm.Base;
using Interface;
using UnityEngine;

namespace Fsm.State.Base
{
    public class DashState : BaseState
    {
        /// 冲刺计时器
        private float _timer;

        /// 冲刺方向
        private Vector3 _dashDir;

        /// 是否从地面开始冲刺
        private bool _startedOnGround;

        public DashState(IPlayerMoveContext moveContext, EventManager eventManager, PlayerStateMachine playerStateMachine) : base(moveContext, eventManager, playerStateMachine) { }

        /// <summary>
        /// 进入冲刺状态：确定方向、标记空中使用、播放动画。
        /// </summary>
        public override void OnEnter()
        {
            base.OnEnter();
            _timer = 0f;

            _startedOnGround = moveContext.Motor.GroundingStatus.IsStableOnGround;

            // 空中冲刺标记已使用并强制脱离地面
            if (!_startedOnGround)
            {
                moveContext.HasUsedAirDash = true;
                moveContext.Motor.ForceUnground();
            }

            // 根据输入确定冲刺方向，无输入则向前
            Vector3 inputDir = moveContext.MoveInputs.moveDirection;
            if (inputDir.sqrMagnitude > 0.01f)
            {
                _dashDir = inputDir.normalized;
                moveContext.Motor.SetRotation(Quaternion.LookRotation(_dashDir, Vector3.up));
            }
            else
            {
                _dashDir = moveContext.Motor.CharacterForward;
            }

            eventManager.Publish(GameEvents.PLAYER_DASH_ANIMATION);
        }

        /// <summary>
        /// 每帧检测跳跃取消和冲刺结束。
        /// </summary>
        /// <param name="deltaTime">帧间隔时间</param>
        public override void OnUpdate(float deltaTime)
        {
            // 冲刺中可以跳跃取消
            if (moveContext.MoveInputs.jumpDown)
            {
                if (moveContext.Motor.GroundingStatus.IsStableOnGround || _startedOnGround)
                {
                    playerStateMachine.TransitionTo(playerStateMachine.JumpState);
                    return;
                }
            }

            _timer += deltaTime;
            if (_timer > moveContext.DashDuration)
            {
                // 根据是否在地面决定切换目标
                if (moveContext.Motor.GroundingStatus.IsStableOnGround)
                {
                    playerStateMachine.TransitionTo(playerStateMachine.MoveState);
                }
                else
                {
                    playerStateMachine.TransitionTo(playerStateMachine.FallState);
                }
            }
        }

        /// <summary>
        /// 以冲刺速度沿冲刺方向移动，地面时贴合斜面。
        /// </summary>
        /// <param name="currentVelocity">当前速度 (引用传递)</param>
        /// <param name="deltaTime">帧间隔时间</param>
        public override void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (moveContext.Motor.GroundingStatus.IsStableOnGround)
            {
                // 投影到斜面
                Vector3 slopeDir = moveContext.Motor.GetDirectionTangentToSurface(_dashDir, moveContext.Motor.GroundingStatus.GroundNormal);
                currentVelocity = slopeDir * moveContext.DashSpeed;
            }
            else
            {
                currentVelocity = _dashDir * moveContext.DashSpeed;
            }
        }
    }
}
using DreamManager;
using DreamSystem.Player;
using Events;
using Fsm.Base;
using Interface;
using UnityEngine;

namespace Fsm.State.Grounded
{
    public class RollState : GroundedState
    {
        /// 翻滚计时器
        private float _timer;

        /// 翻滚方向
        private Vector3 _rollDir;

        public RollState(IPlayerMoveContext moveContext, EventManager eventManager, PlayerStateMachine playerStateMachine) : base(moveContext, eventManager, playerStateMachine) { }

        /// <summary>
        /// 进入翻滚状态：确定翻滚方向并播放动画。
        /// </summary>
        public override void OnEnter()
        {
            base.OnEnter();
            _timer = 0;

            // 根据输入确定翻滚方向，无输入则向前
            _rollDir = moveContext.MoveInputs.moveDirection.normalized;
            if (_rollDir == Vector3.zero)
            {
                _rollDir = moveContext.Motor.CharacterForward;
            }

            eventManager.Publish(GameEvents.PLAYER_ROLL_ANIMATION);
        }

        /// <summary>
        /// 以翻滚速度沿翻滚方向移动。
        /// </summary>
        /// <param name="velocity">当前速度 (引用传递)</param>
        /// <param name="deltaTime">帧间隔时间</param>
        public override void OnUpdateVelocity(ref Vector3 velocity, float deltaTime)
        {
            base.OnUpdateVelocity(ref velocity, deltaTime);
            velocity = _rollDir * moveContext.RollSpeed;
        }

        /// <summary>
        /// 每帧检测跌落和翻滚结束。
        /// </summary>
        /// <param name="deltaTime">帧间隔时间</param>
        public override void OnUpdate(float deltaTime)
        {
            if (CheckFalling()) return;

            _timer += deltaTime;
            if (_timer > moveContext.RollDuration)
            {
                playerStateMachine.TransitionTo(playerStateMachine.MoveState);
            }
        }
    }
}
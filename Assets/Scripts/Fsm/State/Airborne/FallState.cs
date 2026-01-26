using DreamManager;
using DreamSystem.Player;
using Events;
using Fsm.Base;
using Interface;
using UnityEngine;

namespace Fsm.State.Airborne
{
    public class FallState : AirborneState
    {
        public FallState(IPlayerMoveContext moveContext, EventManager eventManager, PlayerStateMachine playerStateMachine) : base(moveContext, eventManager, playerStateMachine) { }

        /// <summary>
        /// 进入下落状态：播放下落动画。
        /// </summary>
        public override void OnEnter()
        {
            base.OnEnter();
            eventManager.Publish(GameEvents.PLAYER_FALL_ANIMATION);
        }

        /// <summary>
        /// 每帧检测落地和空中闪避。
        /// </summary>
        /// <param name="deltaTime">帧间隔时间</param>
        public override void OnUpdate(float deltaTime)
        {
            if (CheckLanding()) return;
            if (CheckAirDash()) return;
        }

        /// <summary>
        /// 继承父类的重力和阻力处理。
        /// </summary>
        /// <param name="currentVelocity">当前速度 (引用传递)</param>
        /// <param name="deltaTime">帧间隔时间</param>
        public override void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            base.OnUpdateVelocity(ref currentVelocity, deltaTime);
        }
    }
}
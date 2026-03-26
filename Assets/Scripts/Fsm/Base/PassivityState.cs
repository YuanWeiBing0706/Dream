using Animancer;
using DreamManager;
using DreamSystem.Player;
using Interface;
using SO;
using UnityEngine;
namespace Fsm.Base
{
    public class PassivityState : BaseState
    {

        /// Animancer 组件，用于播放动画
        protected readonly AnimancerComponent animancerComponent;
        
        /// 角色动画数据
        protected readonly CharacterAnimationSo characterAnimationSo;

        protected AnimancerState currentAnimState;

        protected PassivityState(IPlayerMoveContext moveContext, EventManager eventManager, PlayerStateMachine playerStateMachine, AnimancerComponent animancerComponent, CharacterAnimationSo characterAnimationSo) : base(moveContext, eventManager, playerStateMachine)
        {
            this.animancerComponent = animancerComponent;
            this.characterAnimationSo = characterAnimationSo;
        }
        
        /// <summary>
        /// 切换到待机状态 (MoveState)。
        /// </summary>
        protected void TransitionToIdle()
        {
            playerStateMachine.TransitionTo(playerStateMachine.MoveState);
        }
        
        
        /// <summary>
        /// 攻击时停止水平移动，只保留垂直速度（重力）。
        /// </summary>
        public override void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            currentVelocity.x = 0f;
            currentVelocity.z = 0f;
        }
    }
}
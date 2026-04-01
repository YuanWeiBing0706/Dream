using Animancer;
using DreamManager;
using DreamSystem.Player;
using Fsm.Base;
using Interface;
using SO;
using UnityEngine;
namespace Fsm.State.Passivity
{
    public class DeadState : PassivityState
    {
        /// 死亡是终态，不允许退出
        public override bool CanExit => false;

        public DeadState(IPlayerMoveContext moveContext, EventManager eventManager, PlayerStateMachine playerStateMachine, AnimancerComponent animancerComponent, CharacterAnimationSo characterAnimationSo) : base(moveContext, eventManager, playerStateMachine, animancerComponent, characterAnimationSo) { }

        public override void OnEnter()
        {
            var dieDate = characterAnimationSo.Die;
            currentAnimState = animancerComponent.Play(dieDate);
            Debug.Log("正在播放死亡动画");
        }


        public override void OnUpdate(float deltaTime)
        {
            if (currentAnimState == null)
            {
                return;
            }

            var time = currentAnimState.NormalizedTime;

            if (time >= 1)
            {
                
            }
        }
    }
}
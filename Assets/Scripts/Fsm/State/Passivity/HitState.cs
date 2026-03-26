using Animancer;
using DreamManager;
using DreamSystem.Player;
using Fsm.Base;
using Interface;
using SO;
using UnityEngine;
namespace Fsm.State.Passivity
{
    public class HitState : PassivityState
    {
        public HitState(IPlayerMoveContext moveContext, EventManager eventManager, PlayerStateMachine playerStateMachine, AnimancerComponent animancerComponent, CharacterAnimationSo characterAnimationSo) :
            base(moveContext, eventManager, playerStateMachine, animancerComponent, characterAnimationSo) { }


        public override void OnEnter()
        {
            var hitData = characterAnimationSo.GetHit;
            currentAnimState = animancerComponent.Play(hitData.Clip);
            Debug.Log("正在播放受击动画");
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
                TransitionToIdle();
            }
        }


    }
}
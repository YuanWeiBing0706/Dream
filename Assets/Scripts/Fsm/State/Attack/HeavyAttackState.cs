using Animancer;
using DreamManager;
using DreamSystem.Player;
using Fsm.Base;
using Interface;
using SO;
namespace Fsm.State.Attack
{
    public class HeavyAttackState : AttackState
    {
        private bool _isDetectionOpen;

        protected override string AttackTypeName => "HeavyAttack";

        /// 当前攻击数据 (包含动画和伤害窗口配置)
        private AttackClipData _currentAttackData;

        public HeavyAttackState(IPlayerMoveContext moveContext, EventManager eventManager, PlayerStateMachine playerStateMachine, IPlayerAttackContext attackContext, AnimancerComponent animancerComponent,
            CharacterAnimationSo characterAnimationSo) : base(moveContext, eventManager, playerStateMachine, attackContext, animancerComponent, characterAnimationSo) { }

        public override void OnEnter()
        {
            ResetComboState();
            ConsumeAttackInput();
            PlayCurrentAttack();
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (currentAnimState == null || currentAttackData == null) return;

            float progress = currentAnimState.NormalizedTime;

            UpdateDetectionWindow(progress);

            if (!animationFinished)
            {
                if (IsInCancelWindow(progress) && HasDodgeInput())
                {
                    if (IsLockedOn())
                    {
                        TransitionToRoll();
                    }
                    else
                    {
                        TransitionToDash();
                    }
                    return;
                }

                if (progress >= BufferWindowStart)
                {
                    canBufferInput = true;
                }

                if (canBufferInput && ConsumeAttackInput())
                {
                    hasBufferedInput = true;
                }

                // 动画播放完毕后处理连招转换
                if (progress >= 1f)
                {
                    animationFinished = true;
                    HandleComboTransition();
                }
            }
            else
            {
                timeoutTimer += deltaTime;

                if (ConsumeAttackInput() && !hasBufferedInput)
                {
                    hasBufferedInput = true;
                    HandleComboTransition();
                }

                if (timeoutTimer >= ComboTimeout)
                {
                    TransitionToIdle();
                }
            }
        }

        protected override int GetAttackCount()
        {
            return characterAnimationSo.HeavyAttacks.Count;
        }

        protected override AttackClipData GetAttackData(int index)
        {
            return characterAnimationSo.HeavyAttacks[index];
        }

        protected override bool ConsumeAttackInput()
        {
            return attackContext.ConsumeHeavyAttack();
        }

        /// <summary>
        /// 处理连招转换：有缓冲输入则播放下一段，否则返回待机。
        /// </summary>
        private void HandleComboTransition()
        {
            if (hasBufferedInput && HasNextCombo())
            {
                CloseDetectionIfOpen();
                PrepareNextCombo();
                PlayCurrentAttack();
            }
            else if (hasBufferedInput)
            {
                TransitionToIdle();
            }
        }
    }
}
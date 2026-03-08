using Animancer;
using DreamManager;
using DreamSystem.Player;
using Fsm.Base;
using Interface;
using SO;

namespace Fsm.State.Attack
{
    /// <summary>
    /// 轻攻击状态，继承 AttackState 并实现轻攻击特有的逻辑。
    /// </summary>
    public class LightAttackState : AttackState
    {
        public LightAttackState(IPlayerMoveContext moveContext, EventManager eventManager, PlayerStateMachine playerStateMachine, IPlayerAttackContext attackContext, AnimancerComponent animancerComponent, CharacterAnimationSo characterAnimationSo)
            : base(moveContext, eventManager, playerStateMachine, attackContext, animancerComponent, characterAnimationSo) { }

        protected override string AttackTypeName => "LightAttack";
        

        /// <summary>
        /// 进入状态时重置连招数据并播放第一段攻击。
        /// </summary>
        public override void OnEnter()
        {
            ResetComboState();
            ConsumeAttackInput();
            PlayCurrentAttack();
        }
        
        /// <summary>
        /// 每帧更新，处理伤害检测窗口、连招缓冲和超时逻辑。
        /// </summary>
        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (currentAnimState == null || currentAttackData == null) return;

            float progress = currentAnimState.NormalizedTime;

            UpdateDetectionWindow(progress);

            if (!animationFinished)
            {
                // 检查取消窗口 + 闪避输入
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

                // 进入缓冲窗口后允许记录输入
                if (progress >= BufferWindowStart)
                {
                    canBufferInput = true;
                }

                // 检测攻击输入并记录缓冲
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
                // 动画结束后的超时计时
                timeoutTimer += deltaTime;

                // 超时窗口内仍可接受输入
                if (ConsumeAttackInput() && !hasBufferedInput)
                {
                    hasBufferedInput = true;
                    HandleComboTransition();
                }

                // 超时后返回待机
                if (timeoutTimer >= ComboTimeout)
                {
                    TransitionToIdle();
                }
            }
        }

        protected override int GetAttackCount()
        {
            return characterAnimationSo.LightAttacks.Count;
        }

        protected override AttackClipData GetAttackData(int index)
        {
            return characterAnimationSo.LightAttacks[index];
        } 

        protected override bool ConsumeAttackInput()
        { 
            return attackContext.ConsumeLightAttack();
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
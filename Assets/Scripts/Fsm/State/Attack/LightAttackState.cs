using Animancer;
using DreamManager;
using DreamSystem.Player;
using Events;
using Fsm.Base;
using Interface;
using SO;

namespace Fsm.State.Attack
{
    public class LightAttackState : AttackState
    {
        /// 是否已开启伤害检测
        private bool _isDetectionOpen;

        public LightAttackState(IPlayerMoveContext moveContext, EventManager eventManager, PlayerStateMachine playerStateMachine, IPlayerAttackContext attackContext, AnimancerComponent animancerComponent, CharacterAnimationSo characterAnimationSo) : base(moveContext, eventManager, playerStateMachine, attackContext, animancerComponent, characterAnimationSo) { }

        /// <summary>
        /// 进入状态时重置连招数据并播放第一段攻击。
        /// </summary>
        public override void OnEnter()
        {
            ResetComboState();
            _isDetectionOpen = false;
            PlayCurrentAttack();
        }

        /// <summary>
        /// 每帧更新，处理连招缓冲、伤害检测和超时逻辑。
        /// </summary>
        /// <param name="deltaTime">帧间隔时间</param>
        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (currentAnimState == null) return;

            // 获取当前动画播放进度
            float progress = currentAnimState.NormalizedTime;

            // 处理伤害检测窗口 (示例: 20%-60% 为有效打击帧)
            UpdateDetectionWindow(progress);

            if (!animationFinished)
            {
                // 检查是否在可取消窗口内按下了闪避
                if (CheckCancelWindow(progress)) return;

                // 进入缓冲窗口后允许记录输入
                if (progress >= BufferWindowStart)
                {
                    canBufferInput = true;
                }

                // 检测攻击输入并记录缓冲
                if (canBufferInput && attackContext.AttackInputs.isLightAttack)
                {
                    hasBufferedInput = true;
                }

                // 动画播放完毕后处理连招转换
                if (progress >= 1f)
                {
                    animationFinished = true;
                    ProcessComboTransition();
                }
            }
            else
            {
                // 动画结束后的超时计时
                timeoutTimer += deltaTime;

                // 超时窗口内仍可接受输入
                if (attackContext.AttackInputs.isLightAttack && !hasBufferedInput)
                {
                    hasBufferedInput = true;
                    ProcessComboTransition();
                }

                // 超时后返回待机
                if (timeoutTimer >= ComboTimeout)
                {
                    ReturnToIdle();
                }
            }
        }

        /// <summary>
        /// 根据动画进度更新伤害检测窗口。
        /// </summary>
        /// <param name="progress">当前动画进度 (0-1)</param>
        private void UpdateDetectionWindow(float progress)
        {
            // 伤害检测窗口: 20% - 60%
            const float hitStart = 0.2f;
            const float hitEnd = 0.6f;

            bool shouldBeOpen = progress >= hitStart && progress < hitEnd;

            if (shouldBeOpen && !_isDetectionOpen)
            {
                // 开启伤害检测
                eventManager.Publish(GameEvents.PLAYER_ATTACK_OPEN_DETECTION);
                _isDetectionOpen = true;
            }
            else if (!shouldBeOpen && _isDetectionOpen)
            {
                // 关闭伤害检测
                eventManager.Publish(GameEvents.PLAYER_ATTACK_CLOSE_DETECTION);
                _isDetectionOpen = false;
            }
        }

        /// <summary>
        /// 处理连招转换逻辑：有缓冲输入则播放下一段，否则返回待机。
        /// </summary>
        private void ProcessComboTransition()
        {
            if (hasBufferedInput && HasNextCombo())
            {
                PrepareNextCombo();
                PlayCurrentAttack();
            }
            else if (hasBufferedInput)
            {
                ReturnToIdle();
            }
        }

        /// <summary>
        /// 播放当前 comboIndex 对应的攻击动画。
        /// </summary>
        private void PlayCurrentAttack()
        {
            // 切换动画时关闭上一段的伤害检测
            if (_isDetectionOpen)
            {
                eventManager.Publish(GameEvents.PLAYER_ATTACK_CLOSE_DETECTION);
                _isDetectionOpen = false;
            }

            if (comboIndex < characterAnimationSo.LightAttacks.Count)
            {
                var clip = characterAnimationSo.LightAttacks[comboIndex];
                currentAnimState = animancerComponent.Play(clip);
                UnityEngine.Debug.Log($"[LightAttackState] 播放轻攻击第 {comboIndex + 1} 段");
            }
            else
            {
                ReturnToIdle();
            }
        }

        /// <summary>
        /// 判断是否还有下一段连招。
        /// </summary>
        /// <returns>有则返回 true</returns>
        private bool HasNextCombo()
        {
            return comboIndex + 1 < characterAnimationSo.LightAttacks.Count;
        }

        /// <summary>
        /// 退出状态时清理动画引用和连招数据，并确保关闭伤害检测。
        /// </summary>
        public override void OnExit()
        {
            // 确保退出时关闭伤害检测
            if (_isDetectionOpen)
            {
                eventManager.Publish(GameEvents.PLAYER_ATTACK_CLOSE_DETECTION);
                _isDetectionOpen = false;
            }

            currentAnimState = null;
            comboIndex = 0;
            hasBufferedInput = false;
        }
    }
}
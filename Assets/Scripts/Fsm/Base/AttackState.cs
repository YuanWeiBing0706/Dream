using Animancer;
using DreamManager;
using DreamSystem.Player;
using Interface;
using SO;

namespace Fsm.Base
{
    public abstract class AttackState : BaseState
    {
        /// 攻击上下文，提供攻击输入数据
        protected readonly IPlayerAttackContext attackContext;

        /// Animancer 组件，用于播放动画
        protected readonly AnimancerComponent animancerComponent;

        /// 角色动画数据
        protected readonly CharacterAnimationSo characterAnimationSo;

        /// 当前播放的动画状态
        protected AnimancerState currentAnimState;

        /// 当前连招段数索引
        protected int comboIndex;

        /// 是否有缓冲的攻击输入
        protected bool hasBufferedInput;

        /// 是否可以开始缓冲输入
        protected bool canBufferInput;

        /// 动画是否已播放完毕
        protected bool animationFinished;

        /// 超时计时器
        protected float timeoutTimer;

        /// 缓冲窗口开始时机 (动画进度百分比)
        protected virtual float BufferWindowStart => 0.5f;

        /// 连招超时时间 (秒)
        protected virtual float ComboTimeout => 0.3f;

        /// 可取消窗口开始时机 (动画进度百分比)
        protected virtual float CancelWindowStart => 0.3f;

        /// 可取消窗口结束时机 (动画进度百分比)
        protected virtual float CancelWindowEnd => 0.8f;

        protected AttackState(IPlayerMoveContext moveContext, EventManager eventManager, PlayerStateMachine playerStateMachine, IPlayerAttackContext attackContext, AnimancerComponent animancerComponent, CharacterAnimationSo characterAnimationSo)
            : base(moveContext, eventManager, playerStateMachine)
        {
            this.attackContext = attackContext;
            this.animancerComponent = animancerComponent;
            this.characterAnimationSo = characterAnimationSo;
        }

        /// <summary>
        /// 重置所有连招相关状态。
        /// </summary>
        protected virtual void ResetComboState()
        {
            comboIndex = 0;
            hasBufferedInput = false;
            canBufferInput = false;
            animationFinished = false;
            timeoutTimer = 0f;
        }

        /// <summary>
        /// 检查是否在可取消窗口内按下了闪避。
        /// </summary>
        /// <param name="progress">当前动画进度 (0-1)</param>
        /// <returns>是否已切换状态</returns>
        protected bool CheckCancelWindow(float progress)
        {
            if (progress >= CancelWindowStart && progress <= CancelWindowEnd)
            {
                if (moveContext.MoveInputs.isDodge)
                {
                    if (moveContext.MoveInputs.isLockedOn)
                    {
                        playerStateMachine.TransitionTo(playerStateMachine.RollState);
                    }
                    else
                    {
                        playerStateMachine.TransitionTo(playerStateMachine.DashState);
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检测是否离开地面，是则切换到下落状态。
        /// </summary>
        /// <returns>是否已切换状态</returns>
        protected bool CheckFalling()
        {
            if (!moveContext.Motor.GroundingStatus.IsStableOnGround)
            {
                playerStateMachine.TransitionTo(playerStateMachine.FallState);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 返回待机状态 (MoveState)。
        /// </summary>
        protected void ReturnToIdle()
        {
            playerStateMachine.TransitionTo(playerStateMachine.MoveState);
        }

        /// <summary>
        /// 准备下一段连招：递增索引并重置标记。
        /// </summary>
        protected void PrepareNextCombo()
        {
            comboIndex++;
            hasBufferedInput = false;
            canBufferInput = false;
            animationFinished = false;
            timeoutTimer = 0f;
        }
    }
}
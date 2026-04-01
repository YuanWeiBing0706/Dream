using Animancer;
using Const;
using DreamManager;
using DreamSystem.Player;
using Interface;
using SO;
using UnityEngine;

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

        /// 当前攻击数据 (包含动画和伤害窗口配置)
        protected AttackClipData currentAttackData;

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

        /// 是否已开启伤害检测
        private bool _isDetectionOpen;

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
        /// 获取攻击动画列表的总数。
        /// </summary>
        protected abstract int GetAttackCount();

        /// <summary>
        /// 根据 comboIndex 获取对应的攻击数据。
        /// </summary>
        protected abstract AttackClipData GetAttackData(int index);

        /// <summary>
        /// 消费攻击输入。
        /// </summary>
        protected abstract bool ConsumeAttackInput();

        /// <summary>
        /// 获取攻击类型名称（用于日志）。
        /// </summary>
        protected abstract string AttackTypeName { get; }

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
            _isDetectionOpen = false;
            currentAttackData = null;
        }

        /// <summary>
        /// 播放当前 comboIndex 对应的攻击动画。
        /// </summary>
        /// <returns>是否成功播放</returns>
        protected virtual bool PlayCurrentAttack()
        {
            if (comboIndex >= GetAttackCount())
            {
                return false;
            }

            currentAttackData = GetAttackData(comboIndex);
            currentAnimState = animancerComponent.Play(currentAttackData.Clip);
            Debug.Log($"[{AttackTypeName}] 播放第 {comboIndex + 1} 段 (伤害窗口: {currentAttackData.HitWindowStart:F2} - {currentAttackData.HitWindowEnd:F2})");
            return true;
        }

        /// <summary>
        /// 根据配置的伤害窗口更新检测状态。
        /// </summary>
        protected virtual void UpdateDetectionWindow(float progress)
        {
            if (currentAttackData == null) return;

            bool shouldBeOpen = IsInHitWindow(progress);

            if (shouldBeOpen && !_isDetectionOpen)
            {
                OpenDetection();
                Debug.Log($"[{AttackTypeName}] 开启伤害检测 (进度: {progress:F2})");
            }
            else if (!shouldBeOpen && _isDetectionOpen)
            {
                CloseDetection();
                Debug.Log($"[{AttackTypeName}] 关闭伤害检测 (进度: {progress:F2})");
            }
        }

        /// <summary>
        /// 判断当前进度是否在伤害窗口内。
        /// </summary>
        protected bool IsInHitWindow(float progress)
        {
            if (currentAttackData == null)
            {
                return false;
            }

            return progress >= currentAttackData.HitWindowStart && progress < currentAttackData.HitWindowEnd;
        }

        /// <summary>
        /// 开启伤害检测。
        /// </summary>
        protected void OpenDetection()
        {
            eventManager.Publish(GameEvents.PLAYER_ATTACK_OPEN_DETECTION);
            _isDetectionOpen = true;
        }

        /// <summary>
        /// 关闭伤害检测。
        /// </summary>
        protected void CloseDetection()
        {
            eventManager.Publish(GameEvents.PLAYER_ATTACK_CLOSE_DETECTION);
            _isDetectionOpen = false;
        }

        /// <summary>
        /// 关闭伤害检测（如果已开启）。
        /// </summary>
        protected void CloseDetectionIfOpen()
        {
            if (_isDetectionOpen)
            {
                CloseDetection();
            }
        }

        /// <summary>
        /// 判断是否还有下一段连招。
        /// </summary>
        protected bool HasNextCombo()
        {
            return comboIndex + 1 < GetAttackCount();
        }

        /// <summary>
        /// 判断当前进度是否在可取消窗口内。
        /// </summary>
        protected bool IsInCancelWindow(float progress)
        {
            return progress >= CancelWindowStart && progress <= CancelWindowEnd;
        }

        /// <summary>
        /// 判断当前是否有闪避输入。
        /// </summary>
        protected bool HasDodgeInput()
        {
            return moveContext.MoveInputs.isDodge;
        }

        /// <summary>
        /// 判断当前是否处于锁定状态。
        /// </summary>
        protected bool IsLockedOn()
        {
            return moveContext.MoveInputs.isLockedOn;
        }

        /// <summary>
        /// 判断是否离开地面。
        /// </summary>
        protected bool IsFalling()
        {
            return !moveContext.Motor.GroundingStatus.IsStableOnGround;
        }

        /// <summary>
        /// 切换到翻滚状态。
        /// </summary>
        protected void TransitionToRoll()
        {
            playerStateMachine.TransitionTo(playerStateMachine.RollState);
        }

        /// <summary>
        /// 切换到冲刺状态。
        /// </summary>
        protected void TransitionToDash()
        {
            playerStateMachine.TransitionTo(playerStateMachine.DashState);
        }

        /// <summary>
        /// 切换到下落状态。
        /// </summary>
        protected void TransitionToFall()
        {
            playerStateMachine.TransitionTo(playerStateMachine.FallState);
        }

        /// <summary>
        /// 切换到待机状态 (MoveState)。
        /// </summary>
        protected void TransitionToIdle()
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

        /// <summary>
        /// 攻击时停止水平移动，只保留垂直速度（重力）。
        /// </summary>
        public override void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            currentVelocity.x = 0f;
            currentVelocity.z = 0f;
        }

        /// <summary>
        /// 清理攻击状态数据。
        /// </summary>
        protected virtual void CleanupAttackState()
        {
            currentAttackData = null;
            currentAnimState = null;
            comboIndex = 0;
            hasBufferedInput = false;
        }

        

        
        /// <summary>
        /// 退出状态时清理数据，并确保关闭伤害检测。
        /// </summary>
        public override void OnExit()
        {
            CloseDetectionIfOpen();
            CleanupAttackState();
        }
    }
}
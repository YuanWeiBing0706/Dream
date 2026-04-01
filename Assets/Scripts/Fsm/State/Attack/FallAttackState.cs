using Animancer;
using Const;
using DreamManager;
using DreamSystem.Player;
using Fsm.Base;
using Interface;
using SO;
using UnityEngine;

namespace Fsm.State.Attack
{
    /// <summary>
    /// 下落攻击状态 (Plunge Attack)
    /// 空中按重攻击触发，播放动画起手后暂停并快速下落，落地后恢复并返回待机。
    /// </summary>
    public class FallAttackState : AttackState
    {
        private enum Phase { Startup, Plunging, Landing }
        private Phase _phase;

        /// 动画暂停点 (起手结束，开始下落的进度)
        private const float PLUNGE_START_PROGRESS = 0.4f;

        /// 快速下落速度
        private const float PLUNGE_VELOCITY = 25f;

        /// 是否已开启伤害检测
        private bool _isDetectionOpen;

        public FallAttackState(
            IPlayerMoveContext moveContext,
            EventManager eventManager,
            PlayerStateMachine playerStateMachine,
            IPlayerAttackContext attackContext,
            AnimancerComponent animancerComponent,
            CharacterAnimationSo characterAnimationSo)
            : base(moveContext, eventManager, playerStateMachine, attackContext, animancerComponent, characterAnimationSo)
        { }

        protected override string AttackTypeName => "FallAttack";

        protected override int GetAttackCount() => characterAnimationSo.JumpAttacks.Count;

        protected override AttackClipData GetAttackData(int index) => characterAnimationSo.JumpAttacks[index];

        protected override bool ConsumeAttackInput() => attackContext.ConsumeFallAttack();

        public override void OnEnter()
        {
            _phase = Phase.Startup;
            _isDetectionOpen = false;
            ConsumeAttackInput();

            // 播放动画
            if (GetAttackCount() > 0)
            {
                currentAttackData = GetAttackData(0);
                currentAnimState = animancerComponent.Play(currentAttackData.Clip);
                Debug.Log($"[{AttackTypeName}] 进入下落攻击");
            }
            else
            {
                Debug.LogWarning("[FallAttackState] 没有配置 JumpAttacks 动画！");
                TransitionToFall();
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            if (currentAnimState == null) return;

            float progress = currentAnimState.NormalizedTime;

            switch (_phase)
            {
                case Phase.Startup:
                    // 播放起手动画，到达暂停点后进入下落阶段
                    if (progress >= PLUNGE_START_PROGRESS)
                    {
                        currentAnimState.IsPlaying = false;  // 暂停动画
                        _phase = Phase.Plunging;
                        OpenDetectionLocal();
                        Debug.Log($"[{AttackTypeName}] 开始快速下落");
                    }
                    break;

                case Phase.Plunging:
                    // 保持暂停，等待落地
                    if (moveContext.Motor.GroundingStatus.IsStableOnGround)
                    {
                        currentAnimState.IsPlaying = true;  // 恢复播放
                        _phase = Phase.Landing;
                        CloseDetectionLocal();
                        Debug.Log($"[{AttackTypeName}] 落地");
                    }
                    break;

                case Phase.Landing:
                    // 播放剩余动画后返回待机
                    if (progress >= 1f)
                    {
                        TransitionToIdle();
                    }
                    break;
            }
        }

        public override void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            switch (_phase)
            {
                case Phase.Startup:
                    // 起手阶段：悬停/减速
                    currentVelocity = Vector3.down * 2f;
                    break;

                case Phase.Plunging:
                    // 下落阶段：快速下落
                    currentVelocity = Vector3.down * PLUNGE_VELOCITY;
                    break;

                case Phase.Landing:
                    // 落地阶段：停止移动
                    currentVelocity = Vector3.zero;
                    break;
            }
        }

        public override void OnExit()
        {
            CloseDetectionLocal();
            currentAnimState = null;
            currentAttackData = null;
        }

        private void OpenDetectionLocal()
        {
            if (!_isDetectionOpen)
            {
                eventManager.Publish(GameEvents.PLAYER_ATTACK_OPEN_DETECTION);
                _isDetectionOpen = true;
            }
        }

        private void CloseDetectionLocal()
        {
            if (_isDetectionOpen)
            {
                eventManager.Publish(GameEvents.PLAYER_ATTACK_CLOSE_DETECTION);
                _isDetectionOpen = false;
            }
        }
    }
}
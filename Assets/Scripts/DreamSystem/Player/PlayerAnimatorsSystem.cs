using Animancer;
using Cysharp.Threading.Tasks;
using DreamManager;
using Events;
using Fsm.State.Grounded;
using Interface;
using SO;
using UnityEngine;

namespace DreamSystem.Player
{
    /// <summary>
    /// 玩家动画系统。
    /// <para>纯逻辑系统（非 MonoBehaviour），由 VContainer 注入生命周期。</para>
    /// <para>职责：根据玩家当前状态播放对应的 Locomotion / 跳跃 / 摔倒 / 冒险等动画。</para>
    /// <para>注：攻击动画由 AttackState 内部直接播放，因为连招系统需要跟踪 NormalizedTime。</para>
    /// </summary>
    public class PlayerAnimationSystem : GameSystem
    {
        /// Animancer 动画控制组件
        private readonly AnimancerComponent _animancer;

        /// 移动上下文 (提供输入和物理数据)
        private readonly IPlayerMoveContext _moveContext;

        /// 玩家状态机 (用于判断当前状态)
        private readonly PlayerStateMachine _playerStateMachine;

        /// 事件管理器
        private readonly EventManager _eventManager;

        /// 角色动画配置数据
        private readonly CharacterAnimationSo _animSoData;

        /// 玩家 Transform (用于计算本地方向)
        private readonly Transform _playerTransform;

        public PlayerAnimationSystem(AnimancerComponent animancer, IPlayerMoveContext moveContext, PlayerStateMachine playerStateMachine, EventManager eventManager, CharacterAnimationSo animSoData)
        {
            _animancer = animancer;
            _moveContext = moveContext;
            _playerStateMachine = playerStateMachine;
            _eventManager = eventManager;
            _animSoData = animSoData;
            _playerTransform = moveContext.Motor.transform;
        }
        
        public override void Start()
        {
            _eventManager.Subscribe(GameEvents.PLAYER_JUMP_ANIMATION, PlayJump);
            _eventManager.Subscribe(GameEvents.PLAYER_DASH_ANIMATION, PlayDash);
            _eventManager.Subscribe(GameEvents.PLAYER_ROLL_ANIMATION, PlayRoll);
            _eventManager.Subscribe(GameEvents.PLAYER_FALL_ANIMATION, PlayFall);
        }

        /// <summary>
        /// 每帧后期更新：根据当前状态更新动画。
        /// </summary>
        /// <remarks>
        /// 注意：攻击动画在 LightAttackState 内部直接播放，
        /// 因为连招系统需要追踪动画进度 (NormalizedTime)。
        /// </remarks>
        public override void LateTick()
        {
            var currentState = _playerStateMachine.StateMachine?.CurrentState;

            // 只在 MoveState 时更新 Locomotion 动画
            if (currentState is MoveState)
            {
                UpdateLocomotion();
            }
        }

        /// <summary>
        /// 更新移动动画 (自由移动/锁定移动)。
        /// </summary>
        private void UpdateLocomotion()
        {
            bool isLocked = _moveContext.MoveInputs.isLockedOn;
            float speed = _moveContext.Motor.Velocity.magnitude;

            // 计算相对于角色的输入方向
            Vector3 worldDir = _moveContext.MoveInputs.moveDirection;
            Vector3 localDir = _playerTransform.InverseTransformDirection(worldDir);
            Vector2 input2D = new Vector2(localDir.x, localDir.z);

            if (isLocked)
            {
                // 锁定状态：使用 2D 混合树
                var state = _animancer.Play(_animSoData.LockedMoveMixer);
                if (state is MixerState<Vector2> mixer)
                {
                    mixer.Parameter = input2D * (speed > 0.1f ? 1f : 0f);
                }
            }
            else
            {
                // 自由状态：使用 1D 混合树
                var state = _animancer.Play(_animSoData.FreeMoveMixer);
                if (state is LinearMixerState mixer)
                {
                    mixer.Parameter = speed;
                }
            }
        }

        /// <summary>
        /// 播放跳跃动画。
        /// </summary>
        private void PlayJump() => _animancer.Play(_animSoData.JumpStart);

        /// <summary>
        /// 播放冲刺动画。
        /// </summary>
        private void PlayDash() => _animancer.Play(_animSoData.Dash);

        /// <summary>
        /// 播放下落动画。
        /// </summary>
        private void PlayFall() => _animancer.Play(_animSoData.Fall);

        /// <summary>
        /// 播放翻滚动画 (根据输入方向选择动画)。
        /// </summary>
        private void PlayRoll()
        {
            // 计算相对于角色的输入方向
            Vector3 worldDir = _moveContext.MoveInputs.moveDirection;
            Vector3 localDir = _playerTransform.InverseTransformDirection(worldDir);
            Vector2 input2D = new Vector2(localDir.x, localDir.z);

            // 根据方向获取对应的翻滚动画
            var clip = _animSoData.RollAnimations.GetClipByDirection(input2D);
            _animancer.Play(clip);
        }
        
        public override void Dispose()
        {
            _eventManager.Unsubscribe(GameEvents.PLAYER_JUMP_ANIMATION, PlayJump).Forget();
            _eventManager.Unsubscribe(GameEvents.PLAYER_DASH_ANIMATION, PlayDash).Forget();
            _eventManager.Unsubscribe(GameEvents.PLAYER_ROLL_ANIMATION, PlayRoll).Forget();
            _eventManager.Unsubscribe(GameEvents.PLAYER_FALL_ANIMATION, PlayFall).Forget();
        }
    }
}
using Animancer;
using Cysharp.Threading.Tasks;
using DreamManager;
using Events;
using Fsm.State.Airborne;
using Fsm.State.Grounded;
using SO;
using UnityEngine;

namespace DreamSystem.Player
{
    public class PlayerAnimationSystem : GameSystem
    {
        private readonly AnimancerComponent _animancer;
        private readonly KccMoveController _kccMoveController;
        private readonly EventManager _eventManager;
        private readonly CharacterAnimationSo _animSoData; // 你的动画数据
        private readonly Transform _playerTransform;
        
        // 缓存当前的 Mixer 状态 (为了在 LateTick 更新参数)
        private LinearMixerState _freeMoveState;
        private MixerState<Vector2> _lockedMoveState;

        public PlayerAnimationSystem(AnimancerComponent animancer, KccMoveController kccMoveController, EventManager eventManager, CharacterAnimationSo animSoData)
        {
            _animancer = animancer;
            _kccMoveController = kccMoveController;
            _eventManager = eventManager;
            _animSoData = animSoData;
            _playerTransform = kccMoveController.kinematicCharacterMotor.transform; 
        }

        public override void Start()
        {
            _eventManager.Subscribe(GameEvents.PLAYER_JUMP_ANIMATION, PlayJump);
            _eventManager.Subscribe(GameEvents.PLAYER_DASH_ANIMATION, PlayDash);
            _eventManager.Subscribe(GameEvents.PLAYER_ROLL_ANIMATION, PlayRoll);
            _eventManager.Subscribe(GameEvents.PLAYER_FALL_ANIMATION, PlayFall);
        }

        public override void LateTick()
        {
            // 这是核心循环！
            // 我们根据 Controller 当前在哪种 State，来决定如何更新动画

            var currentState = _kccMoveController.StateMachine.CurrentState;

            // 如果当前在 MoveState (地面移动)
            if (currentState is MoveState)
            {
                UpdateLocomotion();
            }
        }

        private void UpdateLocomotion()
        {
            // 1. 准备数据
            bool isLocked = _kccMoveController.currentInputs.isLockedOn; // 假设你有这个
            float speed = _kccMoveController.kinematicCharacterMotor.Velocity.magnitude;

            // 把输入转换成相对相机的本地坐标 (用于 2D 混合树)
            // 这里简化写，你需要确保 KccInputs 里有这个，或者在这里现算
            Vector3 worldDir = _kccMoveController.currentInputs.moveDirection;
            Vector3 localDir = _playerTransform.InverseTransformDirection(worldDir);
            Vector2 input2D = new Vector2(localDir.x, localDir.z); // x=Right, z=Forward

            // 2. 播放逻辑
            if (isLocked)
            {
                // 播放 2D 锁定移动
                var state = _animancer.Play(_animSoData.LockedMoveMixer);
                if (state is MixerState<Vector2> mixer)
                {
                    // 传入 Vector2，控制前后左右
                    // 这里可能需要根据 speed 做一个映射，保证动画幅度匹配
                    mixer.Parameter = input2D * (speed > 0.1f ? 1f : 0f);
                }
            }
            else
            {
                // 播放 1D 自由移动
                var state = _animancer.Play(_animSoData.FreeMoveMixer);
                if (state is LinearMixerState mixer)
                {
                    // 传入 Float，控制 Idle-Walk-Run
                    mixer.Parameter = speed;
                }
            }
        }


        private void PlayJump()
        {
            _animancer.Play(_animSoData.JumpStart);
        }

        private void PlayDash()
        {
            _animancer.Play(_animSoData.Dash); 
        }

        private void PlayFall()
        {
            _animancer.Play(_animSoData.Fall);
        }

        private void PlayRoll()
        {
            // 锁定翻滚 (需要判断方向)
            // 1. 获取相对输入方向
            Vector3 worldDir = _kccMoveController.currentInputs.moveDirection;
            Vector3 localDir = _playerTransform.InverseTransformDirection(worldDir);
            Vector2 input2D = new Vector2(localDir.x, localDir.z);

            // 2. 从我们写的 DirectionalSet 里取动画
            var clip = _animSoData.RollAnimations.GetClipByDirection(input2D);

            // 3. 播放
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
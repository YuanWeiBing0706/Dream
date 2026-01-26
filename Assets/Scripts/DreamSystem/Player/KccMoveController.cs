using Cysharp.Threading.Tasks;
using DreamManager;
using Events;
using Interface;
using KinematicCharacterController;
using Struct;
using UnityEngine;

namespace DreamSystem.Player
{
    public class KccMoveController : GameSystem, ICharacterController, IPlayerMoveContext
    {
        /// KCC 物理引擎组件
        private readonly KinematicCharacterMotor _motor;

        /// 事件管理器
        private readonly EventManager _eventManager;

        /// 玩家状态机
        private readonly PlayerStateMachine _playerStateMachine;

        /// 攻击上下文 (用于传递给状态机初始化)
        private readonly IPlayerAttackContext _attackContext;

        /// 移动输入缓存
        private MoveInputs _moveInputs;

        /// 是否启用调试日志
        public bool enableLogs = true;

        /// 地面最大移动速度
        public float maxStableMoveSpeed = 10f;

        /// 地面移动平滑度
        public float stableMovementSharpness = 15f;

        /// 空中最大移动速度
        public float maxAirMoveSpeed = 10f;

        /// 空中加速度
        public float airAccelerationSpeed = 10f;

        /// 空气阻力
        public float drag = 0.1f;

        /// 旋转速度 (度/秒)
        public float rotationSpeed = 720f;

        /// 跳跃初速度
        public float jumpSpeed = 10f;

        /// 跳跃预输入容差时间
        public float jumpPreGroundingGraceTime = 0.1f;

        /// 土狼时间 (离开地面后仍可跳跃的时间)
        public float jumpPostGroundingGraceTime = 0.1f;

        /// 重力向量
        public Vector3 gravity = new Vector3(0, -30f, 0);

        /// 翻滚速度
        public float rollSpeed = 10f;

        /// 冲刺速度
        public float dashSpeed = 15f;

        /// 冲刺持续时间
        public float dashDuration = 0.35f;

        /// 翻滚持续时间
        public float rollDuration = 0.8f;

        /// 是否已使用空中冲刺
        private bool _hasUsedAirDash = false;

        /// 是否已使用二段跳
        private bool _hasUsedJump = false;

        // ===== IPlayerMoveContext 实现 =====
        public MoveInputs MoveInputs => _moveInputs;
        public KinematicCharacterMotor Motor => _motor;
        public float MaxStableMoveSpeed => maxStableMoveSpeed;
        public float StableMovementSharpness => stableMovementSharpness;
        public float MaxAirMoveSpeed => maxAirMoveSpeed;
        public float AirAccelerationSpeed => airAccelerationSpeed;
        public float Drag => drag;
        public float RotationSpeed => rotationSpeed;
        public float JumpSpeed => jumpSpeed;
        public float JumpPreGroundingGraceTime => jumpPreGroundingGraceTime;
        public float JumpPostGroundingGraceTime => jumpPostGroundingGraceTime;
        public Vector3 Gravity => gravity;
        public float RollSpeed => rollSpeed;
        public float DashSpeed => dashSpeed;
        public float DashDuration => dashDuration;
        public float RollDuration => rollDuration;
        public bool HasUsedAirDash { get => _hasUsedAirDash; set => _hasUsedAirDash = value; }
        public bool HasUsedJump { get => _hasUsedJump; set => _hasUsedJump = value; }

        public KccMoveController(KinematicCharacterMotor motor, EventManager eventManager, PlayerStateMachine playerStateMachine, IPlayerAttackContext attackContext)
        {
            _motor = motor;
            _eventManager = eventManager;
            _playerStateMachine = playerStateMachine;
            _attackContext = attackContext;
        }

        /// <summary>
        /// 系统启动：注册 KCC 回调、订阅输入事件、初始化状态机。
        /// </summary>
        public override void Start()
        {
            _motor.CharacterController = this;
            KinematicCharacterSystem.Settings.Interpolate = true;
            _eventManager.Subscribe<MoveInputs>(GameEvents.SET_INPUTS, OnSetMoveInputs);

            // 初始化状态机，传入移动和攻击上下文
            _playerStateMachine.Initialize(this, _attackContext);
            _playerStateMachine.ShowDebugLog = enableLogs;
        }

        /// <summary>
        /// 接收移动输入事件。
        /// </summary>
        /// <param name="inputs">移动输入数据</param>
        private void OnSetMoveInputs(MoveInputs inputs)
        {
            _moveInputs = inputs;
        }

        /// <summary>
        /// 每帧后期更新：驱动状态机 Update。
        /// </summary>
        public override void LateTick()
        {
            _playerStateMachine.StateMachine?.CurrentState?.OnUpdate(Time.deltaTime);
        }

        /// <summary>
        /// 更新速度回调 (由 KCC 物理系统调用)。
        /// </summary>
        /// <param name="currentVelocity">当前速度 (引用传递)</param>
        /// <param name="deltaTime">帧间隔时间</param>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            _playerStateMachine.StateMachine?.CurrentState?.OnUpdateVelocity(ref currentVelocity, deltaTime);
        }

        /// <summary>
        /// 更新旋转回调 (由 KCC 物理系统调用)。
        /// </summary>
        /// <param name="currentRotation">当前旋转 (引用传递)</param>
        /// <param name="deltaTime">帧间隔时间</param>
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            _playerStateMachine.StateMachine?.CurrentState?.OnUpdateRotation(ref currentRotation, deltaTime);
        }

        public void AfterCharacterUpdate(float deltaTime) { }
        public void BeforeCharacterUpdate(float deltaTime) { }
        public void PostGroundingUpdate(float deltaTime) { }
        public bool IsColliderValidForCollisions(Collider coll) => true;
        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
        public void OnDiscreteCollisionDetected(Collider hitCollider) { }

        /// <summary>
        /// 释放资源，取消事件订阅。
        /// </summary>
        public override void Dispose()
        {
            _eventManager.Unsubscribe<MoveInputs>(GameEvents.SET_INPUTS, OnSetMoveInputs).Forget();
        }
    }
}
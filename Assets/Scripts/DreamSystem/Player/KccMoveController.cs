using Cysharp.Threading.Tasks;
using DreamManager;
using Events;
using KinematicCharacterController;
using StateMachine;
using StateMachine.State;
using Struct;
using UnityEngine;

namespace DreamSystem.Player
{
    public class KccMoveController : GameSystem, ICharacterController
    {

        /// KCC 物理马达引用。
        public KinematicCharacterMotor kinematicCharacterMotor;
        /// 事件管理器引用
        private readonly EventManager _eventManager;

        /// 移动状态机实例
        public MovementStateMachine StateMachine { get; private set; }

        /// 默认移动状态实例
        public DefaultState defaultState;

        /// 当前帧的输入数据缓存。
        public KccInputs currentInputs;

        /// 地面最大移动速度
        public float maxStableMoveSpeed = 10f;
        /// 地面移动的惯性平滑度
        public float stableMovementSharpness = 15f;

        /// 空中最大移动速度
        public float maxAirMoveSpeed = 10f;
        /// 空中变向的加速度灵敏度
        public float airAccelerationSpeed = 10f;
        /// 空气阻力
        public float drag = 0.1f;

        /// 角色旋转速度 (度/秒)
        public float rotationSpeed = 720f;

        /// 跳跃向上的初速度
        public float jumpSpeed = 10f;
        /// 跳跃前的预输入缓冲时间
        public float jumpPreGroundingGraceTime = 0.1f;
        /// 离开地面后的土狼时间（允许跳跃的宽限期）
        public float jumpPostGroundingGraceTime = 0.1f;

        /// 全局重力向量
        public Vector3 gravity = new Vector3(0, -30f, 0);


        /// <summary>
        /// 构造函数，注入依赖项
        /// </summary>
        /// <param name="kinematicCharacterMotor">KCC 物理马达</param>
        /// <param name="eventManager">事件管理器</param>
        public KccMoveController(KinematicCharacterMotor kinematicCharacterMotor, EventManager eventManager)
        {
            this.kinematicCharacterMotor = kinematicCharacterMotor;
            _eventManager = eventManager;
        }

        /// <summary>
        /// 系统启动时的初始化逻辑
        /// </summary>
        public override void Start()
        {
            // 告诉物理引擎我是司机
            kinematicCharacterMotor.CharacterController = this;
            KinematicCharacterSystem.Settings.Interpolate = true;
            _eventManager.Subscribe<KccInputs>(GameEvents.SET_INPUTS, OnSetInputs);

            // 初始化状态机
            StateMachine = new MovementStateMachine();

            // 创建具体的 State 实例，将 'this' (Controller本身) 传进去，这样 State 就能访问上面的配置和 Motor
            defaultState = new DefaultState(this);

            // 启动默认状态
            StateMachine.Initialize(defaultState);
        }

        /// <summary>
        /// 当接收到输入更新事件时的回调
        /// </summary>
        /// <param name="inputs">新的输入数据包</param>
        private void OnSetInputs(KccInputs inputs)
        {
            currentInputs = inputs;
        }

        /// <summary>
        /// 每帧的后期更新逻辑，驱动状态机更新
        /// </summary>
        public override void LateTick()
        {
            // 驱动当前状态的逻辑更新 (例如检测按键切换状态)
            StateMachine.CurrentState?.OnUpdate(Time.deltaTime);
        }

        // ================= ICharacterController 物理接口实现 =================

        /// <summary>
        /// 更新角色的速度向量。由 KCC 物理马达自动调用。
        /// </summary>
        /// <param name="currentVelocity">当前速度向量（引用）</param>
        /// <param name="deltaTime">时间步长</param>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            StateMachine.CurrentState?.OnUpdateVelocity(ref currentVelocity, deltaTime);
        }

        /// <summary>
        /// 更新角色的旋转朝向。由 KCC 物理马达自动调用。
        /// </summary>
        /// <param name="currentRotation">当前旋转四元数（引用）</param>
        /// <param name="deltaTime">时间步长</param>
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            StateMachine.CurrentState?.OnUpdateRotation(ref currentRotation, deltaTime);
        }

        /// <summary>
        /// 在角色物理更新之后调用
        /// </summary>
        /// <param name="deltaTime">时间步长</param>
        public void AfterCharacterUpdate(float deltaTime)
        {
            // TODO: 如果随后需要实现状态的后处理逻辑（如计时器更新、状态后摇处理等），请在此处调用 State.OnAfterUpdate
        }

        /// <summary>
        /// 在角色物理更新之前调用
        /// </summary>
        /// <param name="deltaTime">时间步长</param>
        public void BeforeCharacterUpdate(float deltaTime)
        {
            // TODO: 如果需要处理物理更新前的预计算（如检测外部强制力、重置特定标记），请在此处编写逻辑
        }

        /// <summary>
        /// 在完成地面检测更新后调用
        /// </summary>
        /// <param name="deltaTime">时间步长</param>
        public void PostGroundingUpdate(float deltaTime)
        {
            // TODO: 如果需要在落地检测完成后立即处理落地音效或着陆动画触发，请在此处编写
        }

        // ================= 碰撞过滤与回调 =================

        /// <summary>
        /// 检查是否应该与特定的碰撞体发生碰撞
        /// </summary>
        /// <param name="coll">目标碰撞体</param>
        /// <returns>如果是 true 则允许碰撞，false 则忽略</returns>
        public bool IsColliderValidForCollisions(Collider coll)
        {
            // TODO: 如果需要忽略特定层的碰撞（如队友、尸体），请在此处添加 Layer 过滤逻辑
            return true;
        }

        /// <summary>
        /// 当与地面发生碰撞时的回调
        /// </summary>
        /// <param name="hitCollider">碰撞到的物体</param>
        /// <param name="hitNormal">碰撞法线</param>
        /// <param name="hitPoint">碰撞点</param>
        /// <param name="hitStabilityReport">稳定性报告引用</param>
        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            // TODO: 如果需要处理落地时的尘土特效或更复杂的物理反馈，请在此处实现
        }

        /// <summary>
        /// 当发生移动碰撞（墙壁、障碍物）时的回调
        /// </summary>
        /// <param name="hitCollider">碰撞到的物体</param>
        /// <param name="hitNormal">碰撞法线</param>
        /// <param name="hitPoint">碰撞点</param>
        /// <param name="hitStabilityReport">稳定性报告引用</param>
        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            // TODO: 如果需要处理撞墙后的反弹或攀爬判定，请在此处实现
        }

        /// <summary>
        /// 处理碰撞稳定性报告
        /// </summary>
        /// <param name="hitCollider">碰撞体</param>
        /// <param name="hitNormal">碰撞法线</param>
        /// <param name="hitPoint">碰撞点</param>
        /// <param name="atCharacterPosition">角色位置</param>
        /// <param name="atCharacterRotation">角色旋转</param>
        /// <param name="hitStabilityReport">稳定性报告引用</param>
        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
            // TODO: 如果需要自定义站立稳定性算法（例如在某些斜坡上滑行），请在此处覆写 hitStabilityReport
        }

        /// <summary>
        /// 当检测到离散碰撞时的回调
        /// </summary>
        /// <param name="hitCollider">碰撞体</param>
        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
            // TODO: 如果需要处理非物理帧的碰撞事件，请在此处实现
        }

        // ================= 销毁 =================

        /// <summary>
        /// 释放资源，取消事件订阅
        /// </summary>
        public override void Dispose()
        {
            _eventManager.Unsubscribe<KccInputs>(GameEvents.SET_INPUTS, OnSetInputs).Forget();
        }
    }
}
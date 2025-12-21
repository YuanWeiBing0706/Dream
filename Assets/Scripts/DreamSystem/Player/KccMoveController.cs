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
        // ================= 核心依赖 =================
        // 这个马达必须公开，因为 State 类需要通过 Controller.kinematicCharacterMotor 来访问它
        public KinematicCharacterMotor kinematicCharacterMotor; 
        private readonly EventManager _eventManager;

        // ================= 状态机系统 =================
        public MovementStateMachine StateMachine { get; private set; }
        
        // 预创建所有状态实例，供状态机切换使用
        public DefaultState defaultState; 

        // ================= 输入数据缓存 =================
        // State 类会在 OnUpdateVelocity 中读取这个变量
        public KccInputs currentInputs; 

        // ================= 全局物理配置参数 =================
        // 这些参数放在这里，是为了让所有的 State 都能读取统一的配置
        // 也可以在 State 内部定义特定参数覆盖这些

        [Header("Ground Movement")] 
        public float maxStableMoveSpeed = 10f; // 地面最大速度
        public float stableMovementSharpness = 15f; // 地面惯性平滑度

        [Header("Air Movement")] 
        public float maxAirMoveSpeed = 10f; // 空中最大移动速度
        public float airAccelerationSpeed = 10f; // 空中变向灵敏度
        public float drag = 0.1f; // 空气阻力

        [Header("Rotation")]
        public float rotationSpeed = 720f; // 转身速度 (度/秒)

        [Header("Jumping")] 
        public float jumpSpeed = 10f; // 跳跃力度
        public float jumpPreGroundingGraceTime = 0.1f; // 预输入缓冲时间
        public float jumpPostGroundingGraceTime = 0.1f; // 土狼时间

        [Header("Environment")] 
        public Vector3 gravity = new Vector3(0, -30f, 0); // 全局重力

        // ================= 构造与初始化 =================

        public KccMoveController(KinematicCharacterMotor kinematicCharacterMotor, EventManager eventManager)
        {
            this.kinematicCharacterMotor = kinematicCharacterMotor;
            _eventManager = eventManager;
        }

        public override void Start()
        {
            // 1. 认主归宗：告诉物理引擎我是司机
            kinematicCharacterMotor.CharacterController = this;
            KinematicCharacterSystem.Settings.Interpolate = true;
            // 2. 订阅输入事件 (接收来自 PlayerMoveSystem 的打包数据)
            _eventManager.Subscribe<KccInputs>(GameEvents.SET_INPUTS, OnSetInputs);

            // 3. 初始化状态机
            StateMachine = new MovementStateMachine();

            // 4. 创建具体的 State 实例
            // 将 'this' (Controller本身) 传进去，这样 State 就能访问上面的配置和 Motor
            defaultState = new DefaultState(this);

            // 5. 启动默认状态
            StateMachine.Initialize(defaultState);
        }

        // 事件回调：只负责更新数据缓存
        private void OnSetInputs(KccInputs inputs)
        {
            currentInputs = inputs;
        }

        public override void LateTick()
        {
            // 驱动当前状态的逻辑更新 (例如检测按键切换状态)
            StateMachine.CurrentState?.OnUpdate(Time.deltaTime);
        }

        // ================= ICharacterController 物理接口实现 =================
        // 下面这些方法由 kinematicCharacterMotor 在 FixedUpdate 时自动调用
        // 我们要做只是把任务“路由”给当前的状态 (CurrentState)

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            StateMachine.CurrentState?.OnUpdateVelocity(ref currentVelocity, deltaTime);
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            StateMachine.CurrentState?.OnUpdateRotation(ref currentRotation, deltaTime);
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            // 如果你的 State 也有后处理逻辑，可以在 State 基类加一个 OnAfterUpdate 并在这里调用
            // 目前暂时留空，或者处理全局的计时器
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
            // 物理更新前的预处理，暂时留空
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            // 落地检测后的处理，暂时留空
        }

        // ================= 碰撞过滤与回调 =================

        public bool IsColliderValidForCollisions(Collider coll)
        {
            // 默认与所有物体碰撞。
            // 如果需要忽略队友或特定层级，在这里写逻辑。
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            // 撞击地面的回调
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            // 撞墙的回调
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
            // 覆写稳定性判断，一般不需要动
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
            // 离散碰撞回调
        }

        // ================= 销毁 =================

        public override void Dispose()
        {
            _eventManager.Unsubscribe<KccInputs>(GameEvents.SET_INPUTS, OnSetInputs).Forget();
        }
    }
}
using Animancer;
using DreamManager;
using Fsm;
using Fsm.Base;
using Fsm.State.Airborne;
using Fsm.State.Attack;
using Fsm.State.Base;
using Fsm.State.Grounded;
using Interface;
using SO;

namespace DreamSystem.Player
{
    public class PlayerStateMachine
    {
        /// 状态机核心驱动
        public StateMachine StateMachine { get; private set; }

        /// 地面移动状态
        public MoveState MoveState { get; private set; }

        /// 翻滚状态
        public RollState RollState { get; private set; }

        /// 冲刺状态
        public DashState DashState { get; private set; }

        /// 跳跃状态
        public JumpState JumpState { get; private set; }

        /// 下落状态
        public FallState FallState { get; private set; }

        /// 轻攻击状态
        public LightAttackState LightAttackState { get; private set; }

        /// 事件管理器
        private readonly EventManager _eventManager;

        /// Animancer 组件
        private readonly AnimancerComponent _animancer;

        /// 角色动画数据
        private readonly CharacterAnimationSo _animData;

        /// 是否显示调试日志
        public bool ShowDebugLog
        {
            get => StateMachine?.ShowDebugLog ?? false;
            set { if (StateMachine != null) StateMachine.ShowDebugLog = value; }
        }

        public PlayerStateMachine(EventManager eventManager, AnimancerComponent animancer, CharacterAnimationSo animData)
        {
            _eventManager = eventManager;
            _animancer = animancer;
            _animData = animData;
        }

        /// <summary>
        /// 初始化状态机和所有状态实例。
        /// </summary>
        /// <param name="moveContext">移动上下文 (由 KccMoveController 实现)</param>
        /// <param name="attackContext">攻击上下文 (由 PlayerAttackSystem 实现)</param>
        public void Initialize(IPlayerMoveContext moveContext, IPlayerAttackContext attackContext)
        {
            StateMachine = new StateMachine();

            // 创建所有状态实例
            MoveState = new MoveState(moveContext, _eventManager, this);
            RollState = new RollState(moveContext, _eventManager, this);
            DashState = new DashState(moveContext, _eventManager, this);
            JumpState = new JumpState(moveContext, _eventManager, this);
            FallState = new FallState(moveContext, _eventManager, this);
            LightAttackState = new LightAttackState(moveContext, _eventManager, this, attackContext, _animancer, _animData);

            // 设置初始状态
            StateMachine.Initialize(MoveState);
        }

        /// <summary>
        /// 切换到指定状态。
        /// </summary>
        /// <param name="newState">目标状态</param>
        public void TransitionTo(BaseState newState)
        {
            StateMachine.TransitionTo(newState);
        }

        /// 当前状态名称
        public string CurrentStateName => StateMachine?.CurrentStateName ?? "None";
    }
}

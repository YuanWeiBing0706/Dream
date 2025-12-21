using StateMachine.Base;
namespace StateMachine
{
    public class MovementStateMachine
    {
        // 当前正在运行的状态
        public MovementState CurrentState { get; private set; }

        // (可选) 仅仅为了在 Inspector Debug 方便看，你可以存个 string name
        public string CurrentStateName => CurrentState != null ? CurrentState.GetType().Name : "None";

        /// <summary>
        /// 初始化状态机
        /// </summary>
        public void Initialize(MovementState startingState)
        {
            CurrentState = startingState;
            CurrentState.OnEnter();
        }

        /// <summary>
        /// 切换状态：Exit 旧的 -> Set 新的 -> Enter 新的
        /// </summary>
        public void TransitionTo(MovementState newState)
        {
            if (newState == null) return;
            if (newState == CurrentState) return; // 已经在该状态就不切了

            CurrentState.OnExit();
            CurrentState = newState;
            CurrentState.OnEnter();
            
            // Debug.Log($"State Changed to: {CurrentStateName}");
        }
    }
}
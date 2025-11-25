using machine;
using VContainer;

public abstract class State
{
    public abstract void OnEnter(StateMachine stateMachine, IContainerBuilder builder);


    public virtual void OnUpdate()
    {

    }

    public abstract void OnExit(StateMachine stateMachine);
}
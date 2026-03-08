using System;

public interface IViewModel
{
    event Action<int> RefreshRequested;

    void OnEnter();
    void OnExit();
}

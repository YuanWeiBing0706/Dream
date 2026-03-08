using System;
using UnityEngine;

public abstract class UIViewBase : MonoBehaviour
{
    public string PanelId { get; private set; }
    public UIKind Kind { get; private set; }
    public IViewModel ViewModel { get; private set; }

    private Action<UIViewBase> _closeCallback;

    public void Initialize(string panelId, UIKind kind, IViewModel viewModel, Action<UIViewBase> closeCallback)
    {
        PanelId = panelId;
        Kind = kind;
        _closeCallback = closeCallback;

        BindViewModel(viewModel);
    }

    public void Open()
    {
        gameObject.SetActive(true);
        OnOpen();
    }

    public void Close()
    {
        OnClose();
        UnbindViewModel();
        Destroy(gameObject);
    }

    protected void RequestClose()
    {
        _closeCallback?.Invoke(this);
    }

    protected virtual void OnOpen()
    {
    }

    protected virtual void OnClose()
    {
    }

    protected virtual void OnViewModelRefreshRequested(int refreshKey)
    {
    }

    protected virtual void OnBind(IViewModel viewModel)
    {
    }

    protected virtual void OnUnbind(IViewModel viewModel)
    {
    }

    private void BindViewModel(IViewModel viewModel)
    {
        UnbindViewModel();

        ViewModel = viewModel;
        if (ViewModel == null)
        {
            return;
        }

        ViewModel.RefreshRequested += OnViewModelRefreshRequestedInternal;
        ViewModel.OnEnter();
        OnBind(ViewModel);
    }

    private void UnbindViewModel()
    {
        if (ViewModel == null)
        {
            return;
        }

        OnUnbind(ViewModel);
        ViewModel.RefreshRequested -= OnViewModelRefreshRequestedInternal;
        ViewModel.OnExit();
        ViewModel = null;
    }

    private void OnViewModelRefreshRequestedInternal(int refreshKey)
    {
        OnViewModelRefreshRequested(refreshKey);
    }
}

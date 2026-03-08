public abstract class UIView<TViewModel> : UIViewBase where TViewModel : class, IViewModel
{
    protected TViewModel VM { get; private set; }

    protected sealed override void OnBind(IViewModel viewModel)
    {
        VM = viewModel as TViewModel;
        OnBindTyped(VM);
    }

    protected sealed override void OnUnbind(IViewModel viewModel)
    {
        OnUnbindTyped(VM);
        VM = null;
    }

    protected virtual void OnBindTyped(TViewModel viewModel)
    {
    }

    protected virtual void OnUnbindTyped(TViewModel viewModel)
    {
    }
}

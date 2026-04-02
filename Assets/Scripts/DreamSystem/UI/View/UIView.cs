using DreamSystem.UI.ViewModel;

namespace DreamSystem.UI.View
{
    public abstract class UIView<TViewModel> : UIViewBase where TViewModel : class, IViewModel
    {
        /// 类型强转后的具体视图模型
        protected TViewModel Vm { get; private set; }

        /// <summary>
        /// 触发抽象层的绑定逻辑并将通用模型转换为强类型实体模型
        /// </summary>
        /// <param name="viewModel">通过抽象层传递过来的模型接口</param>
        protected override sealed void OnBind(IViewModel viewModel)
        {
            // 缓存强转后的类型
            Vm = viewModel as TViewModel;
            OnBindTyped(Vm);
        }

        /// <summary>
        /// 触发抽象层的解绑逻辑并清空泛型缓存引用
        /// </summary>
        /// <param name="viewModel">将要断开的基础模型</param>
        protected override sealed void OnUnbind(IViewModel viewModel)
        {
            OnUnbindTyped(Vm);
            // 清理缓存防止闭包泄漏
            Vm = null;
        }

        /// <summary>
        /// 强类型模型绑定时的钩子虚方法
        /// </summary>
        /// <param name="viewModel">已完成绑定的强类型模型</param>
        protected virtual void OnBindTyped(TViewModel viewModel)
        {
            // TODO: [子类实现与该强类型模型的事件或数据绑定]。
        }

        /// <summary>
        /// 强类型模型解绑时的钩子虚方法
        /// </summary>
        /// <param name="viewModel">即将解除绑定的强类型模型</param>
        protected virtual void OnUnbindTyped(TViewModel viewModel)
        {
            // TODO: [子类实现针对该强类型模型的事件撤销或资源回收]。
        }
    }
}

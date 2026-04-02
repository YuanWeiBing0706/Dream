using System;
using DreamSystem.UI.ViewModel;
using Enum.UI;
using UnityEngine;
namespace DreamSystem.UI.View
{
    public abstract class UIViewBase : MonoBehaviour
    {
        /// 界面的唯一标识符
        public string PanelId { get; private set; }
        /// 界面的种类（全屏或弹窗）
        public UIKind Kind { get; private set; }
        /// 绑定的基础视图模型接口
        public IViewModel ViewModel { get; private set; }

        /// 关闭界面时触发的内部回调
        private Action<UIViewBase> _closeCallback;

        /// <summary>
        /// 初始化界面的核心配置并绑定 ViewModel
        /// </summary>
        /// <param name="panelId">分配给此界面的 ID</param>
        /// <param name="kind">当前派生的界面类型</param>
        /// <param name="viewModel">视图数据模型基类</param>
        /// <param name="closeCallback">此界面关闭时需执行的回调传递</param>
        public void Initialize(string panelId, UIKind kind, IViewModel viewModel, Action<UIViewBase> closeCallback)
        {
            PanelId = panelId;
            Kind = kind;
            _closeCallback = closeCallback;

            // 绑定生命周期系统
            BindViewModel(viewModel);
        }

        /// <summary>
        /// 启用的主干逻辑，激活 GameObject 并调用虚方法
        /// </summary>
        public void Open()
        {
            this.gameObject.SetActive(true);
            OnOpen();
        }

        /// <summary>
        /// 销毁逻辑，调用离场方法，解绑数据节点并销毁挂载的物体
        /// </summary>
        public void Close()
        {
            OnClose();
            // 解绑模型减少底层泄漏
            UnbindViewModel();
            // 委托给UIManager进行处理回收
            RequestClose();
        }

        /// <summary>
        /// 供子类主动通过事件发起自身的关闭请求
        /// </summary>
        protected void RequestClose()
        {
            _closeCallback?.Invoke(this);
        }

        /// <summary>
        /// 当界面被开启时的内部虚方法扩展点
        /// </summary>
        protected virtual void OnOpen()
        {
            // TODO: [子类具体实现该界面刚被建立显示时的动效或逻辑预留]。
        }

        /// <summary>
        /// 当界面被关闭时的内部虚方法扩展点
        /// </summary>
        protected virtual void OnClose()
        {
            // TODO: [子类具体实现该界面被销毁前的资源清理或动效逻辑预留]。
        }

        /// <summary>
        /// 响应数据端发出的刷新指令的钩子方法
        /// </summary>
        /// <param name="refreshKey">携带需要针对性刷新的枚举标志</param>
        protected virtual void OnViewModelRefreshRequested(int refreshKey)
        {
            // TODO: [子类根据 refreshKey 具体重绘文本、控件或进度的表现预留]。
        }

        /// <summary>
        /// 视图模型绑定时的钩子方法
        /// </summary>
        /// <param name="viewModel">刚绑定的模型</param>
        protected virtual void OnBind(IViewModel viewModel)
        {
            // TODO: [子类需要根据绑定好的强类型模型进一步处理局部逻辑预留]。
        }

        /// <summary>
        /// 视图模型解绑时的钩子方法
        /// </summary>
        /// <param name="viewModel">即将解除绑定的模型</param>
        protected virtual void OnUnbind(IViewModel viewModel)
        {
            // TODO: [子类断开对 ViewModel 中额外事件注册的资源清理预留]。
        }

        /// <summary>
        /// 将逻辑核心与此显示核心建立事件纽带
        /// </summary>
        /// <param name="viewModel">将要对接的新 ViewModel</param>
        private void BindViewModel(IViewModel viewModel)
        {
            UnbindViewModel();

            ViewModel = viewModel;
            if (ViewModel == null)
            {
                return;
            }

            // 监听数据层刷新指令
            ViewModel.RefreshRequested += OnViewModelRefreshRequestedInternal;
            ViewModel.OnEnter();
            OnBind(ViewModel);
        }

        /// <summary>
        /// 剥离已有 ViewModels 的关联，阻断事件
        /// </summary>
        private void UnbindViewModel()
        {
            if (ViewModel == null)
            {
                return;
            }

            OnUnbind(ViewModel);
            // 从核心刷新事件池中反注册
            ViewModel.RefreshRequested -= OnViewModelRefreshRequestedInternal;
            ViewModel.OnExit();
            ViewModel = null;
        }

        /// <summary>
        /// 模型发出刷新请求时的中心转发器
        /// </summary>
        /// <param name="refreshKey">刷新识别码</param>
        private void OnViewModelRefreshRequestedInternal(int refreshKey)
        {
            OnViewModelRefreshRequested(refreshKey);
        }
    }
}

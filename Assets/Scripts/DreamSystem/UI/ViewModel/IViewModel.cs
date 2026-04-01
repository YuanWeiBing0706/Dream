using System;

namespace Test.Scripts.UISystem.ViewModel
{
    public interface IViewModel
    {
        /// 当需要主动通知 View 层重新拉取核心数据时呼出的刷新事件
        event Action<int> RefreshRequested;

        /// <summary>
        /// 当此模型正式绑定到活动界面时执行生命周期入口
        /// </summary>
        void OnEnter();

        /// <summary>
        /// 当界面关闭且与此模型解绑时触发的清理工作入口
        /// </summary>
        void OnExit();
    }
}

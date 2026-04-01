using System;
using System.Collections.Generic;

namespace Test.Scripts.UISystem.ViewModel
{
    public abstract class ViewModelBase : IViewModel
    {
        /// 当不需要指定小局部刷新时的默认完整刷新常量键
        public const int FULL_REFRESH_KEY = -1;

        /// 实现接口：主动投递给 View 层的数据更新信号
        public event Action<int> RefreshRequested;

        /// <summary>
        /// 当此模型投入运行环境时的准备工作虚方法
        /// </summary>
        public virtual void OnEnter() 
        { 
            // TODO: [子类具体实现初始化或针对外部底层系统的事件订阅预留]。
        }

        /// <summary>
        /// 当此模型生命结束需断开连接时的虚方法预留
        /// </summary>
        public virtual void OnExit() 
        { 
            // TODO: [子类具体实现清理对象及断开外部事件监听避免泄漏预留]。
        }

        /// <summary>
        /// 变更内部属性的安全方法，值改变时才会发生赋值，避免无意义的消耗
        /// </summary>
        /// <typeparam name="T">期望改变的数据类型</typeparam>
        /// <param name="field">底层存储所使用字段的引用</param>
        /// <param name="value">将要赋给该字段的新值</param>
        /// <returns>值发生改变返回 true，未修改返回 false</returns>
        protected bool SetProperty<T>(ref T field, T value)
        {
            // 通过默认比较器检测新老数值是否一致
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            return true;
        }

        /// <summary>
        /// 变更内部属性并自动发出带 Key 刷新事件的快捷方法
        /// </summary>
        /// <typeparam name="T">预期数据类型</typeparam>
        /// <param name="field">原字段的引用</param>
        /// <param name="value">准备写入的新值</param>
        /// <param name="refreshKey">当值发生改变时所携带抛出的目标刷新指令代码</param>
        /// <returns>值发生改变则返回 true，否则 false</returns>
        protected bool SetProperty<T>(ref T field, T value, int refreshKey)
        {
            bool changed = SetProperty(ref field, value);
            if (changed)
            {
                // 值实际发生了变动，主动向外推送消息
                RefreshRequested?.Invoke(refreshKey);
            }

            return changed;
        }

        /// <summary>
        /// 主动对外抛出刷新界面的请求通知，不指定键则视为全面重绘
        /// </summary>
        /// <param name="refreshKey">代表某局部刷新目标的整形指令码</param>
        protected void NotifyRefresh(int refreshKey = FULL_REFRESH_KEY)
        {
            RefreshRequested?.Invoke(refreshKey);
        }

        /// <summary>
        /// 通过枚举类型触发刷新通知的类型安全重载
        /// </summary>
        /// <typeparam name="TEnum">必须为结构体类型的枚举</typeparam>
        /// <param name="refreshKey">具体的枚举指令</param>
        protected void NotifyRefresh<TEnum>(TEnum refreshKey) where TEnum : struct, System.Enum
        {
            RefreshRequested?.Invoke(Convert.ToInt32(refreshKey));
        }
    }
}
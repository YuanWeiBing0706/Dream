namespace System
{
    public abstract class GameSystem
    {
        /// <summary>
        /// 初始化游戏系统
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// 手动初始化游戏系统，可以传入自定义参数
        /// </summary>
        /// <param name="objs">初始化参数数组</param>
        public virtual void ManualInit(object[] objs)
        {
            
        }

        /// <summary>
        /// 手动释放游戏系统资源
        /// </summary>
        public abstract void ManualDispose();
    }
}
using Cysharp.Threading.Tasks;
using DreamManager;

namespace DreamConfig
{
    public abstract class Config
    {
        /// <summary>
        /// 异步加载配置数据。
        /// </summary>
        /// <param name="resourcesManager">资源管理器实例（由 LoadAllConfig 传入）</param>
        /// <returns>异步任务</returns>
        public abstract UniTask LoadConfig(ResourcesManager resourcesManager);
    }
}
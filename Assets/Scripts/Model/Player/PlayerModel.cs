using Cysharp.Threading.Tasks;
using DreamConfig;
using DreamManager;
using DreamSystem.Damage.Stat;
using Enum.Buff;
using Interface;
using Providers;
using UnityEngine;
using VContainer;

namespace Model.Player
{
    /// <summary>
    /// 玩家模型
    /// <para>实现 IBuffOwner 接口，使 BuffSystem 和 BuffBaseLogic 能够访问玩家的 GameObject 和属性。</para>
    /// </summary>
    public class PlayerModel : MonoBehaviour, IBuffOwner
    {
        public GameObject GameObject => gameObject;

        public CharacterStats Stats { get; private set; }
        
        [Inject] CharacterStatsFactory _factory;
        [Inject] ResourcesManager _resources;

        private void Start()
        {
            InitStatsAsync().Forget();
        }

        private async UniTaskVoid InitStatsAsync()
        {
            // 如果是在 Editor 直接运行 Main 场景，需要等待异步加载完毕
            await UniTask.WaitUntil(() => _resources.GetConfig<CharacterStatsConfig>() != null);

            Stats = _factory.Create("player");
            Debug.Log($"[PlayerModel] 属性初始化成功，当前基础最大血量：{Stats.GetStat(StatType.Health).BaseValue}");
        }
    }
}

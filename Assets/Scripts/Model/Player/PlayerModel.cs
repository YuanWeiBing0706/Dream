using Cysharp.Threading.Tasks;
using Data;
using DreamConfig;
using DreamManager;
using DreamSystem.Damage;
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

        /// 属性初始化完毕后触发（供 PlayerStatusViewModel 等订阅）
        public event System.Action OnStatsInitialized;

        [Inject] CharacterStatsFactory _factory;
        [Inject] ResourcesManager _resources;
        [Inject] GameSessionData _sessionData;
        [Inject] BuffSystem _buffSystem;

        private void Start()
        {
            if (_buffSystem == null)
            {
                Debug.LogError("[PlayerModel] BuffSystem 注入失败，请检查 MainGameScope 是否正确注册。");
                return;
            }
            // 打破循环依赖：BuffSystem 构造时不注入 IBuffOwner，此处手动设置
            _buffSystem.SetOwner(this);
            InitStatsAsync().Forget();
        }

        /// <summary>
        /// 场景卸载 / 对象销毁时显式清除所有 Buff。
        /// 确保 ConvertStatBuffLogic 等订阅了 OnStatChanged 的逻辑都能正确反订阅，
        /// 防止跨场景内存泄漏。
        /// </summary>
        private void OnDestroy()
        {
            _buffSystem?.ClearAllBuff();
        }

        private async UniTaskVoid InitStatsAsync()
        {
            // 等待配置加载完毕（直接从 Editor 运行 Battle 场景时需要）
            await UniTask.WaitUntil(() => _resources.GetConfig<CharacterStatsConfig>() != null);

            Stats = _factory.Create(_sessionData.SelectedCharacterId);
            if (Stats == null)
            {
                Debug.LogWarning($"[PlayerModel] 找不到角色配置: {_sessionData.SelectedCharacterId}，使用 player 属性兜底");
                Stats = _factory.Create("player");
            }

            // 同步持久化血量
            if (_sessionData.CurrentHp > 0)
            {
                Stats.SetCurrentStatValue(StatType.Health, _sessionData.CurrentHp);
            }
            else
            {
                _sessionData.CurrentHp = Stats.GetCurrentStatValue(StatType.Health);
            }

            // 监听血量变化同步到 Session
            Stats.OnCurrentStatChanged += (type, value) =>
            {
                if (type == StatType.Health)
                    _sessionData.CurrentHp = value;
            };

            // 重新应用已解锁的海克斯 Buff（buffId 支持 '|' 分隔的多 ID）
            var hexConfig  = _resources.GetConfig<HexConfig>();
            foreach (var hexId in _sessionData.UnlockedHexIds)
            {
                if (hexConfig != null && hexConfig.TryGet(hexId, out var hexData))
                    ApplyBuffIdField(hexData.buffId);
            }

            // 重新应用已拥有的道具 Buff（跨关卡持久化，死亡时 ResetSession 会清空）
            var itemConfig = _resources.GetConfig<ItemConfig>();
            foreach (var itemId in _sessionData.OwnedItemIds)
            {
                if (itemConfig != null && itemConfig.TryGet(itemId, out var itemData)
                    && !string.IsNullOrEmpty(itemData.buffId))
                {
                    ApplyBuffIdField(itemData.buffId);
                }
            }

            Debug.Log($"[PlayerModel] 属性初始化成功，HP：{Stats.GetCurrentStatValue(StatType.Health)}/{Stats.GetStat(StatType.Health).FinalValue}");
            OnStatsInitialized?.Invoke();
        }

        /// <summary>
        /// 支持 '|' 分隔的多 buffId 字段，逐个调用 AddBuff。
        /// </summary>
        private void ApplyBuffIdField(string buffIdField)
        {
            if (string.IsNullOrEmpty(buffIdField)) return;
            foreach (var id in buffIdField.Split('|'))
            {
                var trimmed = id.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    _buffSystem.AddBuff(trimmed);
            }
        }
    }
}

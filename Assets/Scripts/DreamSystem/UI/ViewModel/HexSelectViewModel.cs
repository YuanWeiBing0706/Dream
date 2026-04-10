using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Data;
using DreamConfig;
using DreamManager;
using DreamSystem.Damage;
using VContainer;

namespace DreamSystem.UI.ViewModel
{
    public class HexSelectViewModel : ViewModelBase
    {
        private readonly GameSessionData _sessionData;
        private readonly ResourcesManager _resources;
        private readonly BuffSystem _buffSystem;

        public ResourcesManager Resources => _resources;

        /// 当前供选择的 3 个海克斯数据
        public List<HexData> Options { get; private set; } = new List<HexData>();

        private UniTaskCompletionSource<bool> _selectionTcs;

        [Inject]
        public HexSelectViewModel(GameSessionData sessionData, ResourcesManager resources, BuffSystem buffSystem)
        {
            _sessionData = sessionData;
            _resources = resources;
            _buffSystem = buffSystem;
        }

        /// <summary>
        /// 按权重随机抽取 3 个可供选择的海克斯（过滤前置条件与已解锁）。
        /// </summary>
        public void RandomizeOptions()
        {
            var config = _resources.GetConfig<HexConfig>();
            if (config == null) return;

            Options.Clear();

            var unlocked = _sessionData.UnlockedHexIds;

            // 只保留"未拥有 且 前置条件已满足"的海克斯
            var available = config.GetAll()
                .Where(h =>
                {
                    if (unlocked.Contains(h.hexId)) return false;
                    string pre = h.prerequisiteHexId;
                    return string.IsNullOrEmpty(pre) || pre == "None" || unlocked.Contains(pre);
                })
                .ToList();

            Options = available.Count <= 3
                ? available
                : WeightedSampleWithoutReplacement(available, 3);

            NotifyRefresh();
        }

        /// <summary>
        /// 等待玩家完成选择，由 LevelManager 在结算流程中 await。
        /// </summary>
        public UniTask WaitForSelection()
        {
            _selectionTcs = new UniTaskCompletionSource<bool>();
            return _selectionTcs.Task;
        }

        /// <summary>
        /// 玩家点击卡牌后调用：记录解锁、应用 Buff，通知流程继续。
        /// buffId 支持 '|' 分隔的多个 ID（如 "buff_hex_kin_3a|buff_hex_kin_3b"）。
        /// </summary>
        public void SelectHex(string hexId)
        {
            var config = _resources.GetConfig<HexConfig>();
            if (!config.TryGet(hexId, out var hexData)) return;

            if (!_sessionData.UnlockedHexIds.Contains(hexId))
                _sessionData.UnlockedHexIds.Add(hexId);

            ApplyBuffIds(hexData.buffId);

            UnityEngine.Debug.Log($"[HexSelect] 玩家选择了海克斯: {hexData.hexName} (ID: {hexId})");

            // 通知 LevelManager 流程继续（关闭 UI 由 View 层负责）
            _selectionTcs?.TrySetResult(true);
        }

        // ─────────────────────────────────────────────────────────────────────────
        // 内部工具方法
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 支持 '|' 分隔的多 buffId 字符串，逐个调用 AddBuff。
        /// </summary>
        private void ApplyBuffIds(string buffIdField)
        {
            if (string.IsNullOrEmpty(buffIdField)) return;
            foreach (var id in buffIdField.Split('|'))
            {
                var trimmed = id.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    _buffSystem.AddBuff(trimmed);
            }
        }

        /// <summary>
        /// 按权重无放回抽样。
        /// </summary>
        private static List<HexData> WeightedSampleWithoutReplacement(List<HexData> pool, int count)
        {
            var result = new List<HexData>(count);
            var remaining = new List<HexData>(pool);

            while (result.Count < count && remaining.Count > 0)
            {
                float total = remaining.Sum(h => h.weight);
                float rand = UnityEngine.Random.Range(0f, total);
                float cumulative = 0f;

                for (int i = 0; i < remaining.Count; i++)
                {
                    cumulative += remaining[i].weight;
                    if (rand <= cumulative)
                    {
                        result.Add(remaining[i]);
                        remaining.RemoveAt(i);
                        break;
                    }
                }
            }
            return result;
        }
    }
}

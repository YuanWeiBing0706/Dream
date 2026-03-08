using System.Collections.Generic;
using Attribute;
using Cysharp.Threading.Tasks;
using DreamManager;
using Function;

namespace DreamConfig
{
    /// <summary>
    /// Buff 配置表。
    /// <para>每行是一条属性修改条目，同一个 BuffId 可以有多行（修改多个属性）。</para>
    /// <para>CSV 格式：BuffId,BuffName,StatType,ModType,Value,Duration</para>
    /// <para>Duration 为 0 表示永久生效（直到手动移除）。</para>
    /// </summary>
    [Config]
    public class BuffConfig : Config
    {
        /// 按 BuffId 分组的修改条目列表
        private readonly Dictionary<string, BuffConfigData> _dic = new();

        public override UniTask LoadConfig(ResourcesManager resourcesManager)
        {
            var textAsset = resourcesManager.LoadAsset<UnityEngine.TextAsset>(nameof(BuffConfig));
            var data = CsvHelper.ReadCsv(textAsset);

            for (int i = 0; i < data.Count; i++)
            {
                var buffId = data[i][0];
                var entry = new BuffModEntry
                {
                    statType = data[i][2],
                    modType = data[i][3],
                    value = float.Parse(data[i][4])
                };

                if (!_dic.ContainsKey(buffId))
                {
                    _dic[buffId] = new BuffConfigData
                    {
                        buffId = buffId,
                        buffName = data[i][1],
                        duration = float.Parse(data[i][5]),
                        modEntries = new List<BuffModEntry>()
                    };
                }
                _dic[buffId].modEntries.Add(entry);
            }

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 按 BuffId 查找 Buff 配置。
        /// </summary>
        public BuffConfigData this[string buffId] => _dic[buffId];

        public bool TryGet(string buffId, out BuffConfigData data) => _dic.TryGetValue(buffId, out data);
    }

    /// <summary>
    /// 单个 Buff 的完整配置（可能包含多条属性修改）。
    /// </summary>
    public class BuffConfigData
    {
        /// Buff ID
        public string buffId;

        /// Buff 名称
        public string buffName;

        /// 持续时间（秒），0 = 永久
        public float duration;

        /// 属性修改条目列表（一个 Buff 可以同时改多个属性）
        public List<BuffModEntry> modEntries;
    }

    /// <summary>
    /// 单条属性修改条目。
    /// </summary>
    public struct BuffModEntry
    {
        /// 修改的属性类型（如 "Attack", "Defense", "Health"）
        public string statType;

        /// 修改方式（如 "Flat", "PercentAdd", "PercentMult"）
        public string modType;

        /// 修改值
        public float value;
    }
}

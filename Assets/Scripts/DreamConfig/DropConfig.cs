using System.Collections.Generic;
using Attribute;
using Cysharp.Threading.Tasks;
using DreamManager;
using Function;

namespace DreamConfig
{
    /// <summary>
    /// 掉落配置表。
    /// <para>每行是一个掉落条目，同一个 DropGroupId 下的多行组成一个掉落池。</para>
    /// <para>CSV 格式：DropGroupId,ItemId,DropWeight,MinCount,MaxCount</para>
    /// <para>掉落权重越高，被选中的概率越大（权重/总权重 = 概率）。</para>
    /// </summary>
    [Config]
    public class DropConfig : Config
    {
        /// 按 DropGroupId 分组的掉落条目列表
        private readonly Dictionary<string, List<DropEntry>> _groups = new();

        public override UniTask LoadConfig(ResourcesManager resourcesManager)
        {
            var textAsset = resourcesManager.LoadAsset<UnityEngine.TextAsset>(nameof(DropConfig));
            var data = CsvHelper.ReadCsv(textAsset);

            for (int i = 0; i < data.Count; i++)
            {
                var entry = new DropEntry
                {
                    dropGroupId = data[i][0],
                    itemId = data[i][1],
                    dropWeight = float.Parse(data[i][2]),
                    minCount = int.Parse(data[i][3]),
                    maxCount = int.Parse(data[i][4])
                };

                if (!_groups.ContainsKey(entry.dropGroupId))
                {
                    _groups[entry.dropGroupId] = new List<DropEntry>();
                }
                _groups[entry.dropGroupId].Add(entry);
            }

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 获取指定掉落组的所有条目。
        /// </summary>
        public List<DropEntry> GetGroup(string dropGroupId)
        {
            return _groups.TryGetValue(dropGroupId, out var entries) ? entries : new List<DropEntry>();
        }

        /// <summary>
        /// 查询掉落组是否存在。
        /// </summary>
        public bool HasGroup(string dropGroupId) => _groups.ContainsKey(dropGroupId);
    }

    /// <summary>
    /// 单条掉落数据。
    /// </summary>
    public struct DropEntry
    {
        /// 掉落组 ID（与 LevelConfig.dropGroupId 关联）
        public string dropGroupId;

        /// 掉落物品 ID
        public string itemId;

        /// 掉落权重（越大概率越高）
        public float dropWeight;

        /// 最小掉落数量
        public int minCount;

        /// 最大掉落数量
        public int maxCount;
    }
}

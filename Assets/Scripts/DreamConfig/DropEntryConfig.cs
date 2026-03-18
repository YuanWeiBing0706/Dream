using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DreamAttribute;
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
    public class DropEntryConfig : Config
    {
        /// 按 DropGroupId 分组的掉落条目列表
        private readonly Dictionary<string, List<DropEntryData>> _dropEntryDir = new Dictionary<string, List<DropEntryData>>();
        
        public override UniTask LoadConfig(ResourcesManager resourcesManager)
        {
            var textAsset = resourcesManager.LoadAsset<UnityEngine.TextAsset>(nameof(DropEntryConfig));
            var data = CsvHelper.ReadCsv(textAsset);

            for (int i = 0; i < data.Count; i++)
            {
                var dropEntryData = new DropEntryData
                {
                    dropGroupId = data[i][0],
                    itemId = data[i][1],
                    dropWeight = float.Parse(data[i][2]),
                    minCount = int.Parse(data[i][3]),
                    maxCount = int.Parse(data[i][4])
                };
                if (!_dropEntryDir.TryGetValue(dropEntryData.dropGroupId, out List<DropEntryData> groupList))
                {
                    groupList = new List<DropEntryData>();
                    _dropEntryDir.Add(dropEntryData.dropGroupId, groupList); 
                }
                
                if (groupList.Exists(entry => entry.itemId == dropEntryData.itemId))
                {
                    UnityEngine.Debug.LogWarning($"[DropConfig] 发现重复配置！掉落组 '{dropEntryData.dropGroupId}' 中已存在道具 '{dropEntryData.itemId}'，已跳过！");
                    continue; 
                }
                groupList.Add(dropEntryData);
            }

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 获取指定掉落组的所有条目。
        /// </summary>
        public List<DropEntryData> GetGroup(string dropGroupId)
        {
            return _dropEntryDir.TryGetValue(dropGroupId, out var entries) ? entries : new List<DropEntryData>();
        }

        /// <summary>
        /// 查询掉落组是否存在。
        /// </summary>
        public bool HasGroup(string dropGroupId) => _dropEntryDir.ContainsKey(dropGroupId);
        
    }

    /// <summary>
    /// 单条掉落数据。
    /// </summary>
    public struct DropEntryData
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

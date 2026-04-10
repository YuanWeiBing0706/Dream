using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DreamAttribute;
using DreamManager;
using Enum.Item;
using Function;
using UnityEngine;

namespace DreamConfig
{
    /// <summary>
    /// 道具配置表。
    /// <para>CSV 格式：itemId,itemGroupId,itemName,itemRarity,weight,prerequisiteItemId,buffId,description,iconPath</para>
    /// </summary>
    [Config]
    public class ItemConfig : Config
    {
        private readonly Dictionary<string, ItemData> _itemDataDir = new();
        private readonly List<ItemData> _allItemList = new();

        public override UniTask LoadConfig(ResourcesManager resourcesManager)
        {
            var textAsset = resourcesManager.LoadAsset<TextAsset>(nameof(ItemConfig));
            var data = CsvHelper.ReadCsv(textAsset);

            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].Length < 7 || string.IsNullOrWhiteSpace(data[i][0])) continue;

                if (!System.Enum.TryParse<ItemRarity>(data[i][3], true, out var rarity))
                {
                    Debug.LogWarning($"[ItemConfig] 无法解析稀有度 '{data[i][3]}'，行 {i} 跳过");
                    continue;
                }

                var itemData = new ItemData
                {
                    itemId              = data[i][0],
                    itemGroupId         = data[i][1],
                    itemName            = data[i][2],
                    itemRarity          = rarity,
                    weight              = float.Parse(data[i][4]),
                    prerequisiteItemId  = data[i][5],
                    buffId              = data[i][6],
                    description         = data[i].Length > 7 ? data[i][7] : string.Empty,
                    iconPath            = data[i].Length > 8 ? data[i][8] : data[i][0]
                };

                if (!_itemDataDir.TryAdd(itemData.itemId, itemData))
                {
                    Debug.LogError($"[ItemConfig] 发现重复道具ID: {itemData.itemId}");
                    continue;
                }
                _allItemList.Add(itemData);
            }

            return UniTask.CompletedTask;
        }

        public ItemData this[string itemId] => _itemDataDir[itemId];

        public bool TryGet(string itemId, out ItemData data) => _itemDataDir.TryGetValue(itemId, out data);

        /// <summary>全部道具（用于加权抽样）。</summary>
        public List<ItemData> GetAllItemList() => _allItemList;

        /// <summary>按稀有度过滤（支持多个稀有度）。</summary>
        public List<ItemData> GetByRarities(params ItemRarity[] rarities)
        {
            var set = new HashSet<ItemRarity>(rarities);
            var result = new List<ItemData>();
            foreach (var item in _allItemList)
            {
                if (set.Contains(item.itemRarity))
                    result.Add(item);
            }
            return result;
        }
    }

    /// <summary>
    /// 道具数据。
    /// </summary>
    public struct ItemData
    {
        /// 道具唯一 ID
        public string itemId;

        /// 道具分组 ID（用于分类展示/查询）
        public string itemGroupId;

        /// 道具名称
        public string itemName;

        /// 稀有度
        public ItemRarity itemRarity;

        /// 掉落权重（越大越容易出现）
        public float weight;

        /// 前置道具 ID（"None" 或空 = 无前置）
        public string prerequisiteItemId;

        /// 关联 Buff ID（领取时挂载此 Buff）
        public string buffId;

        /// 道具描述
        public string description;

        /// 图标资源名（YooAsset 地址，默认与 itemId 相同）
        public string iconPath;
    }
}

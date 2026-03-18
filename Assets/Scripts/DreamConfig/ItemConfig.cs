using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DreamAttribute;
using DreamManager;
using Enum.Item;
using Function;

namespace DreamConfig
{
    /// <summary>
    /// 道具配置表。
    /// <para>定义所有道具的基础信息和效果（掉落物、商店道具共用）。</para>
    /// <para>CSV 格式：ItemId,ItemName,ItemType,Price,SellPrice,BuffId,Description</para>
    /// </summary>
    [Config]
    public class ItemConfig : Config
    {
        private readonly Dictionary<string, ItemData> _itemDataDir = new();
        
        private readonly List<ItemData> _allItemList = new List<ItemData>();

        public override UniTask LoadConfig(ResourcesManager resourcesManager)
        {
            var textAsset = resourcesManager.LoadAsset<UnityEngine.TextAsset>(nameof(ItemConfig));
            
            var data = CsvHelper.ReadCsv(textAsset);

            for (int i = 0; i < data.Count; i++)
            {
                var itemData = new ItemData
                {
                    itemId = data[i][0],
                    itemName = data[i][1],
                    itemUsageType = System.Enum.Parse<ItemUsageType>(data[i][2], true),
                    itemRarity = System.Enum.Parse<ItemRarity>(data[i][3], true),
                    price = int.Parse(data[i][4]),
                    sellPrice = int.Parse(data[i][5]),
                    buffId = data[i][6],
                    description = data[i][7]
                };
                if (!_itemDataDir.TryAdd(itemData.itemId, itemData)) {
                    UnityEngine.Debug.LogError($"[ItemConfig] 发现重复道具ID: {itemData.itemId}");
                    continue;
                }
                _allItemList.Add(itemData);
            }

            return UniTask.CompletedTask;
        }

        public ItemData this[string itemId]
        {
            get
            {
                return _itemDataDir[itemId];
            }
        }
        
        public bool TryGet(string itemId, out ItemData data) => _itemDataDir.TryGetValue(itemId, out data);
        
        public List<ItemData> GetAllItemList() => _allItemList;
        
        
    }

    /// <summary>
    /// 道具数据。
    /// </summary>
    public struct ItemData
    {
        /// 道具 ID
        public string itemId;

        /// 道具名称
        public string itemName;

        /// 道具类型：consumable（主动使用）/ passive（被动生效）
        public ItemUsageType itemUsageType;

        /// 道具稀有度
        public ItemRarity itemRarity;
        
        /// 购买价格（0 = 不可购买）
        public int price;

        /// 出售价格
        public int sellPrice;

        /// 关联的 Buff ID（生效时挂载此 Buff）
        public string buffId;

        /// 道具描述
        public string description;
    }
}

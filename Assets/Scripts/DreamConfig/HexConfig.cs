using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DreamAttribute;
using DreamManager;
using Enum.Hex;
using Function;
using UnityEngine;

namespace DreamConfig
{
    /// <summary>
    /// 海克斯强化配置表。
    /// <para>CSV 格式：hexId,hexGroupId,hexName,hexRarity,weight,prerequisiteHexId,buffId,description</para>
    /// </summary>
    [Config]
    public class HexConfig : Config
    {
        private readonly Dictionary<string, HexData> _dic = new();
        private readonly Dictionary<string, List<HexData>> _byTier = new();

        public override UniTask LoadConfig(ResourcesManager resourcesManager)
        {
            var textAsset = resourcesManager.LoadAsset<TextAsset>(nameof(HexConfig));
            var data = CsvHelper.ReadCsv(textAsset);

            // CsvHelper 已跳过标题行，从 i=0 开始遍历数据行
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].Length < 8 || string.IsNullOrWhiteSpace(data[i][0])) continue;

                var hexData = new HexData
                {
                    hexId           = data[i][0],
                    hexGroupId      = data[i][1],
                    hexName         = data[i][2],
                    hexRarity       = System.Enum.Parse<HexRarity>(data[i][3], true),
                    weight          = float.Parse(data[i][4]),
                    prerequisiteHexId = data[i][5],
                    buffId          = data[i][6],
                    iconPath        = "",          // CSV 暂无此列，预留字段
                    description     = data[i][7]
                };

                if (!_dic.TryAdd(hexData.hexId, hexData))
                {
                    UnityEngine.Debug.LogError($"[HexConfig] 发现重复的海克斯ID: {hexData.hexId}");
                    continue;
                }

                // 按稀有度分组
                string rarityStr = hexData.hexRarity.ToString();
                if (!_byTier.TryGetValue(rarityStr, out List<HexData> groupList))
                {
                    groupList = new List<HexData>();
                    _byTier.Add(rarityStr, groupList);
                }
                groupList.Add(hexData);
            }

            return UniTask.CompletedTask;
        }

        public HexData this[string hexId] => _dic[hexId];

        public bool TryGet(string hexId, out HexData data) => _dic.TryGetValue(hexId, out data);

        /// <summary>
        /// 获取指定稀有度的所有海克斯（用于三选一刷新池）。
        /// </summary>
        public List<HexData> GetByTier(string tier)
        {
            return _byTier.TryGetValue(tier, out var list) ? list : new List<HexData>();
        }

        /// <summary>
        /// 获取所有海克斯（用于随机抽取）。
        /// </summary>
        public List<HexData> GetAll()
        {
            return new List<HexData>(_dic.Values);
        }

        /// <summary>
        /// 获取同组的所有海克斯（用于查找升级路线）。
        /// </summary>
        public List<HexData> GetByGroup(string hexGroupId)
        {
            return _dic.Values.Where(h => h.hexGroupId == hexGroupId).ToList();
        }
    }

    public struct HexData
    {
        public string hexId;
        public string hexGroupId;
        public string hexName;
        public HexRarity hexRarity;
        public float weight;
        public string prerequisiteHexId;
        public string buffId;
        public string iconPath;
        public string description;
    }
}

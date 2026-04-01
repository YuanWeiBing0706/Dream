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
    /// <para>每行是一个独立的海克斯条目，同名海克斯通过 HexGroupId 关联多个等级。</para>
    /// <para>CSV 格式：HexId,HexGroupId,HexName,HexType,Tier,Weight,PrerequisiteHexId,Description,参数...</para>
    /// <para>HexType 为海克斯逻辑类的类名（通过反射创建实例）。</para>
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

            for (int i = 0; i < data.Count; i++)
            {
                var hexData = new HexData
                {
                    hexId = data[i][0],
                    hexGroupId = data[i][1],
                    hexName = data[i][2],
                    hexRarity = System.Enum.Parse<HexRarity>(data[i][3], true),
                    weight = float.Parse(data[i][4]),
                    prerequisiteHexId = data[i][5],
                    buffId = data[i][6],
                    description = data[i][7]
                };

                // 经典 TryAdd 防崩大法
                if (!_dic.TryAdd(hexData.hexId, hexData))
                {
                    UnityEngine.Debug.LogError($"[HexConfig] 发现重复的海克斯ID: {hexData.hexId}");
                    continue;
                }
    
                // 如果你还需要按稀有度分组，也可以用我们刚刚讲过的 List 字典模板！
            }

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 按 HexId 查找。
        /// </summary>
        public HexData this[string hexId] => _dic[hexId];

        public bool TryGet(string hexId, out HexData data) => _dic.TryGetValue(hexId, out data);

        /// <summary>
        /// 获取指定等级的所有海克斯（用于三选一刷新池）。
        /// </summary>
        public List<HexData> GetByTier(string tier)
        {
            return _byTier.TryGetValue(tier, out var list) ? list : new List<HexData>();
        }

        /// <summary>
        /// 获取同组的所有海克斯（用于查找升级路线）。
        /// </summary>
        public List<HexData> GetByGroup(string hexGroupId)
        {
            return _dic.Values.Where(h => h.hexGroupId == hexGroupId).ToList();
        }
    }

    /// <summary>
    /// 海克斯强化数据。
    /// </summary>
    public struct HexData
    {
        /// 海克斯唯一 ID（如 "hex_money_w"）
        public string hexId;

        /// 海克斯组 ID（用于判定是否拿过同系列的，如 "hex_money"）
        public string hexGroupId;

        /// 显示名称
        public string hexName;
    
        /// 海克斯稀有度（白、金、彩）
        public HexRarity hexRarity;

        /// 刷新权重（控制随机出现的概率）
        public float weight;
    
        /// 前置海克斯 ID（空 = 无前置，天生可刷新）
        public string prerequisiteHexId;

        /// 对应的BuffId
        public string buffId;
    
        /// UI 描述
        public string description;
    }
}

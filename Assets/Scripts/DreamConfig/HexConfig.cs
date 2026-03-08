using System;
using System.Collections.Generic;
using System.Linq;
using Attribute;
using Cysharp.Threading.Tasks;
using DreamManager;
using Function;

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
            var textAsset = resourcesManager.LoadAsset<UnityEngine.TextAsset>(nameof(HexConfig));
            var data = CsvHelper.ReadCsv(textAsset);

            for (int i = 0; i < data.Count; i++)
            {
                // 解析参数列（第 8 列开始，格式同 ItemConfig：_fieldName[type]:value）
                var properties = new List<HexProperty>();
                for (int j = 8; j < data[i].Length; j++)
                {
                    var line = data[i][j];
                    if (string.IsNullOrEmpty(line)) continue;

                    string[] parts = line.Split(new[] { ':' }, 2);
                    string definitionPart = parts[0];
                    string value = parts.Length > 1 ? parts[1] : null;
                    string pName = null;
                    string type = null;

                    int openBracket = definitionPart.IndexOf('[');
                    int closeBracket = definitionPart.IndexOf(']');

                    if (openBracket != -1 && closeBracket != -1 && openBracket < closeBracket)
                    {
                        pName = definitionPart.Substring(0, openBracket);
                        type = definitionPart.Substring(openBracket + 1, closeBracket - openBracket - 1);
                    }

                    properties.Add(new HexProperty { name = pName, type = type, value = value });
                }

                var hexData = new HexData
                {
                    hexId = data[i][0],
                    hexGroupId = data[i][1],
                    hexName = data[i][2],
                    hexType = Type.GetType(data[i][3]),
                    tier = data[i][4],
                    weight = float.Parse(data[i][5]),
                    prerequisiteHexId = data[i][6],
                    description = data[i][7],
                    properties = properties
                };
                _dic.Add(hexData.hexId, hexData);

                if (!_byTier.ContainsKey(hexData.tier))
                {
                    _byTier[hexData.tier] = new List<HexData>();
                }
                _byTier[hexData.tier].Add(hexData);
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

        /// 海克斯组 ID（同名不同等级共享，如 "hex_money"）
        public string hexGroupId;

        /// 显示名称
        public string hexName;

        /// 海克斯逻辑类的类型（通过反射创建实例）
        public Type hexType;

        /// 等级：white / gold / rainbow
        public string tier;

        /// 同等级池子内的刷新权重
        public float weight;

        /// 前置海克斯 ID（空 = 无前置，天生可刷新）
        public string prerequisiteHexId;

        /// 描述
        public string description;

        /// 构造参数列表（反射注入到逻辑类的字段）
        public List<HexProperty> properties;
    }

    /// <summary>
    /// 海克斯参数（格式同骰子项目的 Property）。
    /// </summary>
    public struct HexProperty
    {
        public string name;
        public string type;
        public string value;
    }
}

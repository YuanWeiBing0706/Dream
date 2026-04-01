using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DreamAttribute;
using DreamManager;
using Enum.Buff;
using Function;
using UnityEngine;

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
        private readonly Dictionary<string, BuffData> _dic = new();

        public override UniTask LoadConfig(ResourcesManager resourcesManager)
        {
            var textAsset = resourcesManager.LoadAsset<TextAsset>("BuffConfig");
            var data = CsvHelper.ReadCsv(textAsset);

            for (int i = 1; i < data.Count; i++)
            {
                // 如果空行跳过
                if (data[i].Length < 10 || string.IsNullOrWhiteSpace(data[i][0])) continue;

                var buffId = data[i][0];

                // 1. 使用 TryGetValue 大法来处理“多行合并”
                if (!_dic.TryGetValue(buffId, out BuffData configData))
                {
                    // 如果是第一次遇到这个 buffId，就新建一个图纸基础信息
                    configData = new BuffData
                    {
                        buffId = buffId,
                        buffName = data[i][1],
                        logicType = System.Enum.Parse<BuffLogicType>(data[i][2], true),
                        duration = float.Parse(data[i][3]),
                        maxStack = int.Parse(data[i][4]),
                        stackType = System.Enum.Parse<StackType>(data[i][5], true),
                        extraParam = data[i][9], // 额外参数，比如控制掩码 "Move|Attack"
                        buffEntryDataList = new List<BuffEntryData>()
                    };
                    _dic.Add(buffId, configData);
                }

                // 2. 解析属性修改部分（针对 ModifyAttribute 类型的多行设计）
                string statTypeStr = data[i][6];

                // 只有当这一行确实配置了属性修改（没填 None 或空）时，才加入集合
                if (!string.IsNullOrEmpty(statTypeStr) && statTypeStr != "None")
                {
                    var entry = new BuffEntryData
                    {
                        statType = System.Enum.Parse<StatType>(statTypeStr, true),
                        modType = System.Enum.Parse<StatModType>(data[i][7], true),
                        value = float.Parse(data[i][8])
                    };
                    configData.buffEntryDataList.Add(entry);
                }
            }

            return UniTask.CompletedTask;
        }


        public bool TryGet(string buffId, out BuffData data) => _dic.TryGetValue(buffId, out data);

    }

    /// <summary>
    /// 单个 Buff 的完整配置（可能包含多条属性修改）。
    /// </summary>
    public struct BuffData
    {
        public string buffId;
        public string buffName;
        public BuffLogicType logicType; // 决定了 Manager 会 new 出哪个 Logic 和 Data 子类

        public float duration;
        public int maxStack;

        public StackType stackType;

        /// 针对加减面板属性的列表（支持多行）
        public List<BuffEntryData> buffEntryDataList;

        /// 针对其他特殊机制的通用字符串（比如 "Move|Attack"、"0.2" 等）
        public string extraParam;
    }

    /// <summary>
    /// 单条属性修改条目。
    /// </summary>
    public struct BuffEntryData
    {
        /// 修改的属性类型（如 "Attack", "Defense", "Health"）
        public StatType statType;

        /// 修改方式（如 "Flat", "PercentAdd", "PercentMult"）
        public StatModType modType;

        /// 修改值
        public float value;

    }

}
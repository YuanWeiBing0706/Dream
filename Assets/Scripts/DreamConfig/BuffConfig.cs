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
    /// <para>CSV 格式（12列）：</para>
    /// <para>buffId(0), buffName(1), logicType(2), duration(3), maxStack(4), stackType(5),
    /// statType(6), sourceStat(7), modType(8), value(9), stringParam(10), description(11)</para>
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

            for (int i = 0; i < data.Count; i++)
            {
                // 至少需要 10 列（0-9 为必填；10=stringParam、11=description 为可选）
                if (data[i].Length < 10 || string.IsNullOrWhiteSpace(data[i][0])) continue;

                var buffId = data[i][0];

                if (!_dic.TryGetValue(buffId, out BuffData configData))
                {
                    configData = new BuffData
                    {
                        buffId      = buffId,
                        buffName    = data[i][1],
                        logicType   = System.Enum.Parse<BuffLogicType>(data[i][2], true),
                        duration    = float.Parse(data[i][3]),
                        maxStack    = int.Parse(data[i][4]),
                        stackType   = System.Enum.Parse<StackType>(data[i][5], true),
                        // 列 10：stringParam（AddFlag 的标记名 / AddControlLock 的锁名）
                        stringParam = data[i].Length > 10 ? data[i][10] : "",
                        buffEntryDataList = new List<BuffEntryData>()
                    };
                    _dic.Add(buffId, configData);
                }

                // ── 解析属性修改部分 ──────────────────────────────────────────
                // 列 6：statType（目标属性，"None" 表示此行无属性修改）
                string statTypeStr = data[i][6];
                if (string.IsNullOrEmpty(statTypeStr) || statTypeStr == "None") continue;

                // 列 7：sourceStat（来源属性，ConvertStat 专用；其余为 "None"）
                string sourceStatStr = data[i][7];
                StatType? sourceStat = (string.IsNullOrEmpty(sourceStatStr) || sourceStatStr == "None")
                    ? (StatType?)null
                    : System.Enum.Parse<StatType>(sourceStatStr, true);

                // 列 8：modType；列 9：value
                var entry = new BuffEntryData
                {
                    statType   = System.Enum.Parse<StatType>(statTypeStr, true),
                    sourceStat = sourceStat,
                    modType    = System.Enum.Parse<StatModType>(data[i][8], true),
                    value      = float.Parse(data[i][9])
                };
                configData.buffEntryDataList.Add(entry);
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
        public BuffLogicType logicType;

        public float duration;
        public int maxStack;

        public StackType stackType;

        /// 属性修改条目列表（ModifyAttribute / ConvertStat 使用）
        public List<BuffEntryData> buffEntryDataList;

        /// 通用字符串参数（AddFlag 的标记名 / AddControlLock 的锁名）
        public string stringParam;
    }

    /// <summary>
    /// 单条属性修改条目。
    /// </summary>
    public struct BuffEntryData
    {
        /// 目标属性（将被修改的属性）
        public StatType statType;

        /// 来源属性（ConvertStat 专用：从此属性取值计算转化量；其他逻辑为 null）
        public StatType? sourceStat;

        /// 修改器计算类型（Flat / PercentAdd / PercentMult）
        public StatModType modType;

        /// 修改值 / 转化系数
        public float value;
    }
}

using System;
using Enum.Buff;
namespace DreamSystem.Damage.Stat
{
    /// <summary>
    /// 属性修改器。
    /// <para>可附加到 BaseStat 上，按类型和优先级修改属性最终值。</para>
    /// <para>支持来源追踪（Source），方便按来源批量移除（如 Buff 到期）。</para>
    /// </summary>
    [Serializable]
    public class StatModifier
    {
        /// 修改器数值
        public readonly float value;

        /// 修改器类型（Flat / PercentAdd / PercentMult）
        public readonly StatModType type;

        /// 计算优先级（数值越小越先计算）
        public readonly int order;

        /// 修改器来源（用于按来源批量移除）
        public readonly object source;

        /// <summary>
        /// 完整构造函数：指定值、类型、优先级和来源。 
        /// </summary>
        public StatModifier(float value, StatModType type, int order, object source)
        {
            this.value = value;
            this.type = type;
            this.order = order;
            this.source = source;
        }

        /// <summary>
        /// 简化构造函数：优先级默认使用类型枚举值。
        /// </summary>
        public StatModifier(float value, StatModType type, object source) : this(value, type, (int)type, source) { }
    }
}

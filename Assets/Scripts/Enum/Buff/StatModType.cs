namespace Enum.Buff
{
    /// <summary>
    /// 属性修改器类型。
    /// <para>决定修改器如何影响基础属性值。</para>
    /// </summary>
    public enum StatModType
    {
        /// 固定值加成（如 +10 攻击力）
        Flat,

        /// 百分比叠加加成（同类相加后乘算，如 +20% 和 +30% → ×1.5）
        PercentAdd,

        /// 百分比独立乘算（逐个乘算，如 ×1.2 × 1.3）
        PercentMult
    }
}

namespace Enum.Buff
{
    public enum StackType
    {
        /// 不可叠加（相同 Buff 不会叠层）
        None,

        /// 全局叠加（不区分来源）
        Aggregate,

        /// 按来源叠加（不同来源独立计算层数）
        AggregateBySource
    }
}
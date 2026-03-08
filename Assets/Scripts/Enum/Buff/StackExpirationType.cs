namespace Enum.Buff
{
    public enum StackExpirationType
    {
        /// 清除所有层数（Buff 直接移除）
        ClearEntireStack,

        /// 减少一层并刷新持续时间
        RemoveSingleStackAndRefreshDuration,

        /// 刷新持续时间（不减层）
        RefreshDuration
    }
}
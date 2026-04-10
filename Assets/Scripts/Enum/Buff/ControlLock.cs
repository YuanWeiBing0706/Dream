using System;

namespace Enum.Buff
{
    /// <summary>
    /// 控制锁定标志位（支持位运算组合）。
    /// <para>用于标识角色可被锁定的行为类型。</para>
    /// </summary>
    [Flags]
    public enum ControlLock
    {
        None   = 0,
        Move   = 1 << 0,
        Attack = 1 << 1,
        Cast   = 1 << 2,
        /// 禁用冲刺/闪避（对应 Hex"舍弃回避"系列）
        Dash   = 1 << 3,
    }
}
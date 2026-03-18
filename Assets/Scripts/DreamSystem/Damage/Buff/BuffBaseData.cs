using System.Collections.Generic;
using Enum.Buff;
using Interface;
namespace DreamSystem.Damage.Buff
{
    /// <summary>
    /// Buff 配置数据（抽象基类）。
    /// <para>定义一个 Buff 的所有静态属性：持续时间、叠加规则、标签、优先级等。</para>
    /// <para>子类可扩展附加功能（如控制锁定、标签黑名单）。</para>
    /// </summary>
    public abstract class BuffBaseData
    {
        /// Buff 唯一标识
        public string buffID;

        /// 执行优先级（数值越大越先执行）
        public int priority = 0;

        /// 持续时间（秒）
        public float duration = 1f;

        /// 最大叠加层数
        public int maxStack = 1;

        /// Buff 标签列表（用于分类查询和批量操作）
        public List<BuffTag> tags = new List<BuffTag>();

        /// 叠加时的持续时间刷新策略
        public StackDurationRefreshPolicy durationRefreshPolicy = StackDurationRefreshPolicy.Refresh;

        /// 叠加类型（是否可叠加、按来源区分等）
        public StackType stackType = StackType.None;

        /// 到期时的层数处理策略
        public StackExpirationType stackExpirationType = StackExpirationType.ClearEntireStack;

        /// 层数溢出时的处理策略（已达最大层数时再次添加）
        public OverflowType overflowType = OverflowType.DontRefreshDuration;
    }
}
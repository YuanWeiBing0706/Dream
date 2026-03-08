using Enum.Buff;
namespace DreamSystem.Damage.Buff
{
    /// <summary>
    /// Buff 运行时实例。
    /// <para>封装一个正在生效的 Buff 的所有运行时状态：剩余时间、当前层数、配置数据和逻辑引用。</para>
    /// </summary>
    public class BuffInstance
    {
        /// Buff 配置数据
        public BuffData data;

        /// Buff 逻辑实例
        public BuffBase logic;

        /// Buff 来源（用于按来源叠加区分）
        public object source;

        /// 所属 BuffContainer（用于 BuffBase 回调操作，如移除黑名单标签）
        public BuffSystem buffSystem;

        /// 剩余持续时间（秒）
        public float remainingTime;

        /// 当前叠加层数
        public int stack;

        /// <summary>
        /// 构造函数：初始化 Buff 实例，设置持续时间和初始层数。
        /// </summary>
        public BuffInstance(BuffData data, BuffBase logic, object source, BuffSystem buffSystem)
        {
            this.data = data;
            this.logic = logic;
            this.source = source;
            this.buffSystem = buffSystem;
            remainingTime = data.duration;
            stack = 1;
        }

        /// <summary>
        /// 每帧/每回合更新：减少剩余时间并调用逻辑 OnUpdate。
        /// </summary>
        public void Tick(float deltaTime)
        {
            remainingTime -= deltaTime;
            logic?.OnUpdate(deltaTime);
        }

        /// <summary>
        /// 判断 Buff 是否已到期。
        /// </summary>
        public bool IsFinished()
        {
            return remainingTime <= 0;
        }

        /// <summary>
        /// 尝试增加一层：未达上限则叠层，已达上限则按溢出策略处理。
        /// </summary>
        public void AddStack()
        {
            if (stack < data.maxStack)
            {
                stack++;
                ApplyDurationPolicy(data.durationRefreshPolicy);
                logic?.OnStackChanged(stack);
                return;
            }

            if (data.overflowType == OverflowType.RefreshDuration)
            {
                RefreshDuration();
            }
        }

        /// <summary>
        /// 尝试处理到期：根据 StackExpirationType 策略决定清除全部层数还是减层。
        /// </summary>
        /// <returns>true 表示 Buff 应被移除</returns>
        public bool TryResolveExpiration()
        {
            if (!IsFinished())
            {
                return false;
            }

            if (data.stackExpirationType == StackExpirationType.ClearEntireStack)
            {
                stack = 0;
                return true;
            }

            if (data.stackExpirationType == StackExpirationType.RemoveSingleStackAndRefreshDuration)
            {
                stack--;
                if (stack <= 0)
                {
                    return true;
                }

                RefreshDuration();
                logic?.OnStackChanged(stack);
                return false;
            }

            RefreshDuration();
            return false;
        }

        /// <summary>
        /// 手动减少一层并通知逻辑。
        /// </summary>
        /// <returns>剩余层数</returns>
        public int MinusStack()
        {
            stack--;
            logic?.OnStackChanged(stack);
            return stack;
        }

        private void ApplyDurationPolicy(StackDurationRefreshPolicy policy)
        {
            if (policy == StackDurationRefreshPolicy.Refresh)
            {
                RefreshDuration();
                return;
            }

            remainingTime += data.duration;
        }

        private void RefreshDuration()
        {
            remainingTime = data.duration;
        }
    }
}

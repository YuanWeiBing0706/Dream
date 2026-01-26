using System;
using DreamManager;
using Events;

namespace Function.Damageable
{
    /// <summary>
    /// 敌人伤害处理器。
    /// <para>每只敌人拥有一个独立的 Handler 实例，用于处理该敌人的受伤逻辑。</para>
    /// <para>职责：扣血 + 通知订阅者。</para>
    /// </summary>
    public class EnemyDamageHandler : IDamageable
    {
        public float MaxHp { get; private set; }
        public float CurrentHp { get; private set; }
        public bool IsDead => CurrentHp <= 0;

        private readonly EventManager _eventManager;

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="maxHp">最大血量</param>
        /// <param name="eventManager">事件管理器（可选，用于发布全局事件）</param>
        public EnemyDamageHandler(float maxHp, EventManager eventManager = null)
        {
            MaxHp = maxHp;
            CurrentHp = maxHp;
            _eventManager = eventManager;
        }

        /// <summary>
        /// 受到伤害。
        /// </summary>
        /// <param name="amount">伤害值</param>
        public void TakeDamage(float amount)
        {
            if (IsDead) return; // 已经死了就不再受伤

            CurrentHp -= amount;

            // 通过 EventManager 发布受伤事件
            _eventManager?.Publish(GameEvents.ENEMY_DAMAGED, new EnemyDamageData(this, amount));

            // 检查死亡
            if (CurrentHp <= 0)
            {
                CurrentHp = 0;
                _eventManager?.Publish(GameEvents.ENEMY_DEATH, this);
            }
        }

        /// <summary>
        /// 重置血量（从对象池复用时调用）。
        /// </summary>
        /// <param name="maxHp">新的最大血量</param>
        public void Reset(float maxHp)
        {
            MaxHp = maxHp;
            CurrentHp = maxHp;
        }
    }
}
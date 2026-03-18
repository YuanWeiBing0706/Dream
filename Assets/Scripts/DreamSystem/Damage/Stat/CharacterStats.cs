using System;
using System.Collections.Generic;
using Enum.Buff;
using UnityEngine;
namespace DreamSystem.Damage.Stat
{
    /// <summary>
    /// 角色属性初始化数据。
    /// <para>用于构造 CharacterStats 时传入基础属性值。</para>
    /// </summary>
    [Serializable]
    public class CharacterStatsInitData
    {
        public float BaseHealth = 100f;
        public float BaseShield = 0f;
        public float BaseAttack = 10f;
        public float BaseDefense = 5f;
        public float BaseSpeed = 5f;
    }
    

    /// <summary>
    /// 角色属性管理器。
    /// <para>纯逻辑类（不依赖 MonoBehaviour），可通过构造函数或 DI 创建。</para>
    /// <para>支持属性修改器（Buff/Debuff）、当前值跟踪、伤害计算（护盾优先消耗）、标记、背包。</para>
    /// </summary>
    public class CharacterStats
    {
        /// 所有属性字典（StatType → BaseStat）
        public Dictionary<StatType, BaseStat> allStatDir = new Dictionary<StatType, BaseStat>();

        /// 属性最终值变化事件（修改器增减时触发）
        public event Action<StatType, float> OnStatChanged;

        /// 属性当前值变化事件（受伤/治疗时触发）
        public event Action<StatType, float> OnCurrentStatChanged;

        /// 通用数据变化事件（用于 UI 精准刷新，key 可为 StatType 或 CharacterDataKey）
        public event Action<int> OnDataChanged;

        /// 属性当前值跟踪（如当前血量、当前护盾）
        private readonly Dictionary<StatType, float> _currentValues = new Dictionary<StatType, float>();

        /// 按来源追踪的护盾值
        private readonly Dictionary<object, float> _shieldValuesBySource = new Dictionary<object, float>();

        /// 角色标记列表
        private readonly List<string> _flags = new List<string>();

        /// 角色背包物品列表
        private readonly List<string> _inventoryItems = new List<string>();

        /// 角色当前拥有的标记，只读视图
        public IReadOnlyList<string> Flags => _flags;
        /// 角色当前包含在背包的物品ID列表，只读视图
        public IReadOnlyList<string> InventoryItems => _inventoryItems;

        /// <summary>
        /// 构造函数：通过初始化数据创建角色属性。
        /// </summary>
        public CharacterStats(CharacterStatsInitData initData = null)
        {
            CharacterStatsInitData data = initData ?? new CharacterStatsInitData();
            InitializeStats(data);
        }

        /// <summary>
        /// 构造函数：通过基础属性值直接创建角色属性（向后兼容）。
        /// </summary>
        public CharacterStats(float health, float shield = 0f, float attack = 10f, float defense = 5f, float speed = 5f)
        {
            InitializeStats(new CharacterStatsInitData
            {
                BaseHealth = health,
                BaseShield = shield,
                BaseAttack = attack,
                BaseDefense = defense,
                BaseSpeed = speed
            });
        }

        /// <summary>
        /// 根据初始化数据配置基础属性字典并设定初始血量和护盾。
        /// </summary>
        private void InitializeStats(CharacterStatsInitData data)
        {
            allStatDir.Add(StatType.Health, new BaseStat(StatType.Health, data.BaseHealth, 0, float.MaxValue, 0));
            allStatDir.Add(StatType.Shield, new BaseStat(StatType.Shield, data.BaseShield, 0, float.MaxValue, 0));
            allStatDir.Add(StatType.Attack, new BaseStat(StatType.Attack, data.BaseAttack, 0, float.MaxValue, 0));
            allStatDir.Add(StatType.Defense, new BaseStat(StatType.Defense, data.BaseDefense, 0, float.MaxValue, 0));
            allStatDir.Add(StatType.Speed, new BaseStat(StatType.Speed, data.BaseSpeed, 0, float.MaxValue, 0));

            _currentValues[StatType.Health] = allStatDir[StatType.Health].FinalValue;
            _currentValues[StatType.Shield] = allStatDir[StatType.Shield].FinalValue;
        }

        // ============================================
        // 数据变化通知
        // ============================================

        /// <summary>
        /// 触发数据变化事件（使用 StatType 作为标识）。
        /// </summary>
        public void NotifyDataChanged(StatType statType)
        {
            NotifyDataChanged((int)statType);
        }

        /// <summary>
        /// 触发数据变化事件（使用 CharacterDataKey 作为标识）。
        /// </summary>
        public void NotifyDataChanged(CharacterDataKey dataKey)
        {
            NotifyDataChanged((int)dataKey);
        }

        /// <summary>
        /// 触发底层数据变化事件（直接使用 int 作为标识）。
        /// </summary>
        public void NotifyDataChanged(int refreshKey)
        {
            OnDataChanged?.Invoke(refreshKey);
        }

        // ============================================
        // 标记系统
        // ============================================

        /// <summary>
        /// 为角色添加指定标记。
        /// </summary>
        /// <param name="flag">需要添加的标记名称</param>
        /// <returns>添加是否成功（若标记为空或已存在，返回 false）</returns>
        public bool AddFlag(string flag)
        {
            if (string.IsNullOrEmpty(flag) || _flags.Contains(flag))
            {
                return false;
            }

            _flags.Add(flag);
            NotifyDataChanged(CharacterDataKey.Flags);
            return true;
        }

        /// <summary>
        /// 为角色移除指定标记。
        /// </summary>
        /// <param name="flag">需要移除的标记名称</param>
        /// <returns>移除是否成功（若标记不存在，返回 false）</returns>
        public bool RemoveFlag(string flag)
        {
            if (string.IsNullOrEmpty(flag))
            {
                return false;
            }

            bool removed = _flags.Remove(flag);
            if (removed)
            {
                NotifyDataChanged(CharacterDataKey.Flags);
            }

            return removed;
        }

        /// <summary>
        /// 检查角色是否拥有指定标记。
        /// </summary>
        /// <param name="flag">需要检查的标记名称</param>
        /// <returns>是否拥有该标记</returns>
        public bool HasFlag(string flag)
        {
            return _flags.Contains(flag);
        }

        // ============================================
        // 背包系统
        // ============================================

        /// <summary>
        /// 批量覆盖设置角色的背包物品列表。
        /// </summary>
        /// <param name="items">新的物品列表</param>
        public void SetInventoryItems(IEnumerable<string> items)
        {
            _inventoryItems.Clear();
            if (items != null)
            {
                _inventoryItems.AddRange(items);
            }

            NotifyDataChanged(CharacterDataKey.InventoryItems);
        }

        /// <summary>
        /// 向角色背包添加一件物品。
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <returns>添加是否成功</returns>
        public bool AddInventoryItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return false;
            }

            _inventoryItems.Add(itemId);
            NotifyDataChanged(CharacterDataKey.InventoryItems);
            return true;
        }

        /// <summary>
        /// 从角色背包中移除指定物品。
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <returns>移除是否成功（若背包无此物品，返回 false）</returns>
        public bool RemoveInventoryItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return false;
            }

            bool removed = _inventoryItems.Remove(itemId);
            if (removed)
            {
                NotifyDataChanged(CharacterDataKey.InventoryItems);
            }

            return removed;
        }

        // ============================================
        // 属性操作
        // ============================================

        /// <summary>
        /// 获取指定类型的属性对象（基础值和修饰器管理对象）。
        /// </summary>
        /// <param name="type">属性类型</param>
        /// <returns>BaseStat 对象，若未注册该类型属性则返回 null</returns>
        public BaseStat GetStat(StatType type)
        {
            if (allStatDir.TryGetValue(type, out BaseStat stat))
            {
                return stat;
            }

            UnityEngine.Debug.LogError($"Stat {type} not found!");
            return null;
        }

        /// <summary>
        /// 向指定属性添加属性修改器（Buff / Debuff 的效果等），并通知事件。
        /// </summary>
        /// <param name="type">目标属性类型</param>
        /// <param name="modifier">修饰器实例</param>
        public void AddStatModifier(StatType type, StatModifier modifier)
        {
            BaseStat stat = GetStat(type);
            if (stat != null)
            {
                float oldValue = stat.FinalValue;
                stat.AddModifier(modifier);
                if (!Mathf.Approximately(oldValue, stat.FinalValue))
                {
                    ClampCurrentValueIfNeeded(type);
                    OnStatChanged?.Invoke(type, stat.FinalValue);
                    NotifyDataChanged(type);
                }
            }
        }

        /// <summary>
        /// 从指定属性移除对应的修饰器，并触发数值变动通知。
        /// </summary>
        /// <param name="type">目标属性类型</param>
        /// <param name="modifier">待移除的修饰器实例</param>
        public void RemoveStatModifier(StatType type, StatModifier modifier)
        {
            BaseStat stat = GetStat(type);
            if (stat != null)
            {
                float oldValue = stat.FinalValue;
                stat.RemoveModifier(modifier);
                if (!Mathf.Approximately(oldValue, stat.FinalValue))
                {
                    ClampCurrentValueIfNeeded(type);
                    OnStatChanged?.Invoke(type, stat.FinalValue);
                    NotifyDataChanged(type);
                }
            }
        }

        // ============================================
        // 当前值操作
        // ============================================

        /// <summary>
        /// 获取某项属性的“当前”值（如当前血量、当前护盾）。不受动态追踪的属性将返回最终上限。
        /// </summary>
        /// <param name="type">属性类型</param>
        /// <returns>当前值</returns>
        public float GetCurrentStatValue(StatType type)
        {
            if (_currentValues.TryGetValue(type, out float value))
            {
                return value;
            }

            BaseStat stat = GetStat(type);
            return stat != null ? stat.FinalValue : 0f;
        }

        /// <summary>
        /// 强制设置当前属性值，确保其被有效钳制在 [0, MaxValue] 之间，并在数值变动时抛出事件通知。
        /// </summary>
        /// <param name="type">目标属性</param>
        /// <param name="value">设定的目标数值</param>
        public void SetCurrentStatValue(StatType type, float value)
        {
            float maxValue = type == StatType.Shield ? float.MaxValue : (GetStat(type)?.FinalValue ?? float.MaxValue);
            float oldValue = GetCurrentStatValue(type);
            float newValue = Mathf.Clamp(value, 0f, maxValue);

            _currentValues[type] = newValue;
            if (!Mathf.Approximately(oldValue, newValue))
            {
                OnCurrentStatChanged?.Invoke(type, newValue);
                NotifyDataChanged(type);
            }
        }

        /// <summary>
        /// 以差值计算改变当前属性的值（负数代表扣减，正数代表回复）。
        /// </summary>
        /// <param name="type">目标属性</param>
        /// <param name="delta">数值变动量</param>
        public void ChangeCurrentStatValue(StatType type, float delta)
        {
            SetCurrentStatValue(type, GetCurrentStatValue(type) + delta);
        }

        // ============================================
        // 伤害与护盾
        // ============================================

        /// <summary>
        /// 应用伤害：先消耗护盾，护盾不足时溢出伤害扣减血量。
        /// </summary>
        /// <returns>实际扣减血量的伤害值</returns>
        public float ApplyDamage(float damage)
        {
            if (damage <= 0f)
            {
                return 0f;
            }

            float remainingDamage = damage;
            if (GetCurrentStatValue(StatType.Shield) > 0f)
            {
                remainingDamage = ConsumeShield(remainingDamage);
            }

            if (remainingDamage > 0f)
            {
                ChangeCurrentStatValue(StatType.Health, -remainingDamage);
            }

            return remainingDamage;
        }

        /// <summary>
        /// 按来源添加护盾。
        /// </summary>
        public void AddShield(float value, object source)
        {
            if (value <= 0f)
            {
                return;
            }

            object key = source ?? this;
            if (_shieldValuesBySource.TryGetValue(key, out float existing))
            {
                _shieldValuesBySource[key] = existing + value;
            }
            else
            {
                _shieldValuesBySource[key] = value;
            }

            ChangeCurrentStatValue(StatType.Shield, value);
        }

        /// <summary>
        /// 按来源移除护盾。
        /// </summary>
        public bool RemoveShieldBySource(object source)
        {
            if (source == null)
            {
                return false;
            }

            if (_shieldValuesBySource.TryGetValue(source, out float remain))
            {
                _shieldValuesBySource.Remove(source);
                ChangeCurrentStatValue(StatType.Shield, -remain);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 清除所有护盾。
        /// </summary>
        public void ClearAllShield()
        {
            _shieldValuesBySource.Clear();
            SetCurrentStatValue(StatType.Shield, 0f);
        }

        /// <summary>
        /// 清除指定来源的所有修改器和护盾。
        /// </summary>
        public void ClearSource(object source)
        {
            if (source == null)
            {
                return;
            }

            foreach (KeyValuePair<StatType, BaseStat> pair in allStatDir)
            {
                float oldValue = pair.Value.FinalValue;
                bool removed = pair.Value.RemoveAllModifiersFromSource(source);
                if (removed && !Mathf.Approximately(oldValue, pair.Value.FinalValue))
                {
                    ClampCurrentValueIfNeeded(pair.Key);
                    OnStatChanged?.Invoke(pair.Key, pair.Value.FinalValue);
                    NotifyDataChanged(pair.Key);
                }
            }

            RemoveShieldBySource(source);
        }

        // ============================================
        // 内部方法
        // ============================================

        /// <summary>
        /// 内部执行消耗护盾的扣减计算。会优先按来源逐个抵消各自建立的护盾池。
        /// </summary>
        /// <param name="damage">需要被抵消的原始伤害值</param>
        /// <returns>抵消护盾后剩余仍然未被吸收的溢出伤害</returns>
        private float ConsumeShield(float damage)
        {
            float shieldDamage = Mathf.Min(damage, GetCurrentStatValue(StatType.Shield));
            float remainingToConsume = shieldDamage;

            List<object> keys = new List<object>(_shieldValuesBySource.Keys);
            foreach (object key in keys)
            {
                if (remainingToConsume <= 0f)
                {
                    break;
                }

                float pool = _shieldValuesBySource[key];
                float consumed = Mathf.Min(pool, remainingToConsume);
                pool -= consumed;
                remainingToConsume -= consumed;

                if (pool <= 0f)
                {
                    _shieldValuesBySource.Remove(key);
                }
                else
                {
                    _shieldValuesBySource[key] = pool;
                }
            }

            ChangeCurrentStatValue(StatType.Shield, -shieldDamage);
            return damage - shieldDamage;
        }

        /// <summary>
        /// 修剪方法：通过检测确保某项属性内部的当前变动值不超过因为附带各种修饰器而变动的该属性最后上限。
        /// </summary>
        /// <param name="type">将受检的目标属性类型</param>
        private void ClampCurrentValueIfNeeded(StatType type)
        {
            if (type == StatType.Shield)
            {
                return;
            }

            if (!_currentValues.TryGetValue(type, out float currentValue))
            {
                return;
            }

            float maxValue = GetStat(type)?.FinalValue ?? float.MaxValue;
            float clamped = Mathf.Clamp(currentValue, 0f, maxValue);
            if (!Mathf.Approximately(currentValue, clamped))
            {
                _currentValues[type] = clamped;
                OnCurrentStatChanged?.Invoke(type, clamped);
            }
        }
    }
}

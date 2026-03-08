using System;
using System.Collections.Generic;
using DreamSystem.Damage.Stat;
using Enum;
using Enum.Buff;
using UnityEngine;
using UnityEngine.Serialization;
namespace DreamSystem.Damage
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

        public IReadOnlyList<string> Flags => _flags;
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

        public void NotifyDataChanged(StatType statType)
        {
            NotifyDataChanged((int)statType);
        }

        public void NotifyDataChanged(CharacterDataKey dataKey)
        {
            NotifyDataChanged((int)dataKey);
        }

        public void NotifyDataChanged(int refreshKey)
        {
            OnDataChanged?.Invoke(refreshKey);
        }

        // ============================================
        // 标记系统
        // ============================================

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

        public bool HasFlag(string flag)
        {
            return _flags.Contains(flag);
        }

        // ============================================
        // 背包系统
        // ============================================

        public void SetInventoryItems(IEnumerable<string> items)
        {
            _inventoryItems.Clear();
            if (items != null)
            {
                _inventoryItems.AddRange(items);
            }

            NotifyDataChanged(CharacterDataKey.InventoryItems);
        }

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

        public BaseStat GetStat(StatType type)
        {
            if (allStatDir.TryGetValue(type, out BaseStat stat))
            {
                return stat;
            }

            UnityEngine.Debug.LogError($"Stat {type} not found!");
            return null;
        }

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

        public float GetCurrentStatValue(StatType type)
        {
            if (_currentValues.TryGetValue(type, out float value))
            {
                return value;
            }

            BaseStat stat = GetStat(type);
            return stat != null ? stat.FinalValue : 0f;
        }

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

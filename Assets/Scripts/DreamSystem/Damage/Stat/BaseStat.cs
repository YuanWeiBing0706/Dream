using System;
using System.Collections.Generic;
using Enum.Buff;
namespace DreamSystem.Damage.Stat
{
    /// <summary>
    /// 基础属性。
    /// <para>管理单个属性的基础值、修改器列表和最终值计算。</para>
    /// <para>计算顺序：Flat(+固定值) → PercentAdd(百分比叠加) → PercentMult(百分比乘算) → Clamp(限制范围)。</para>
    /// </summary>
    [Serializable]
    public class BaseStat
    {
        /// 属性类型
        public StatType Type;

        /// 基础值（未加任何修改器的原始值）
        public float BaseValue;

        /// 属性最小值限制
        private float _minValue;

        /// 属性最大值限制
        private float _maxValue;

        /// 小数点精度（用于四舍五入）
        private int _decimalPlaces;

        /// 经过所有修改器计算后的最终值
        private float _finalValue;

        /// 属性最终值（只读）
        public float FinalValue => _finalValue;

        /// 当前生效的修改器列表
        private List<StatModifier> _modifierList = new List<StatModifier>();

        /// <summary>
        /// 构造函数：初始化属性类型、基础值、范围和精度。
        /// </summary>
        public BaseStat(StatType type, float baseValue, float minValue, float maxValue, int decimalPlaces = 2)
        {
            Type = type;
            BaseValue = baseValue;
            _minValue = minValue;
            _maxValue = maxValue;
            _decimalPlaces = Math.Max(0, decimalPlaces);
            CalculateFinalValue();
        }

        /// <summary>
        /// 添加修改器并重新计算最终值。
        /// </summary>
        public void AddModifier(StatModifier modifier)
        {
            _modifierList.Add(modifier);
            _modifierList.Sort((a, b) => a.order.CompareTo(b.order));
            CalculateFinalValue();
        }

        /// <summary>
        /// 移除指定修改器并重新计算最终值。
        /// </summary>
        public void RemoveModifier(StatModifier modifier)
        {
            _modifierList.Remove(modifier);
            CalculateFinalValue();
        }

        /// <summary>
        /// 移除指定来源的所有修改器（如 Buff 到期时批量清除）。
        /// </summary>
        /// <param name="source">修改器来源对象</param>
        /// <returns>是否有修改器被移除</returns>
        public bool RemoveAllModifiersFromSource(object source)
        {
            int removedCount = _modifierList.RemoveAll(m => ReferenceEquals(m.source, source));
            if (removedCount > 0)
            {
                CalculateFinalValue();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 重新计算最终值：Flat → PercentAdd → PercentMult → Clamp → Round。
        /// </summary>
        private void CalculateFinalValue()
        {
            float value = BaseValue;

            foreach (var mod in _modifierList)
            {
                if (mod.type == StatModType.Flat && mod.order == 0)
                {
                    value += mod.value;
                }
            }

            float sumPercentAdd = 0;
            foreach (var mod in _modifierList)
            {
                if (mod.type == StatModType.PercentAdd && mod.order == 1)
                {
                    sumPercentAdd += mod.value;
                }
            }
            value *= (1 + sumPercentAdd);
            foreach (var mod in _modifierList)
            {
                if (mod.type == StatModType.PercentMult && mod.order == 2)
                {
                    value *= (1 + mod.value);
                }
            }
            value = Math.Clamp(value, _minValue, _maxValue);
            float precision = (float)Math.Pow(10f, _decimalPlaces);
            _finalValue = (float)Math.Round(value * precision) / precision;
        }
    }
}
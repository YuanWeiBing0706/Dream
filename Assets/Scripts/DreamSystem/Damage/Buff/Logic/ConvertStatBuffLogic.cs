using DreamAttribute;
using DreamConfig;
using DreamSystem.Damage.Buff.Data;
using DreamSystem.Damage.Stat;
using Enum.Buff;

namespace DreamSystem.Damage.Buff.Logic
{
    /// <summary>
    /// 属性转化型 Buff 逻辑。
    /// <para>
    /// 将 <c>sourceStat</c> 的最终值按 <c>value</c> 系数转化，
    /// 作为 Flat 修改器叠加到 <c>statType</c>（目标属性）上。
    /// </para>
    /// <para>
    /// 动态监听：每当 sourceStat 变化时，移除旧修改器并添加新的。
    /// 内置重入防护，防止 sourceStat → targetStat → sourceStat 循环触发 Stack Overflow。
    /// </para>
    /// <para>CSV 示例：Attack, Speed, Flat, 0.2 → 攻击力 += 速度 × 20%</para>
    /// </summary>
    [BuffLogic(BuffLogicType.ConvertStat)]
    public class ConvertStatBuffLogic : BuffBaseLogic
    {
        private StatType _targetStat;
        private StatType _sourceStat;
        private float _coefficient;
        private StatModifier _currentMod;

        /// 重入防护标志：防止 ApplyConversion 触发 OnStatChanged 再次递归进入
        private bool _applying;

        public override void OnApply()
        {
            if (buffInstance.data is not AttributeBuffData attrData || attrData.buffEntryDataList.Count == 0)
            {
                UnityEngine.Debug.LogWarning("[ConvertStatBuffLogic] 未找到有效的 BuffEntryData");
                return;
            }

            var entry = attrData.buffEntryDataList[0];
            _targetStat  = entry.statType;
            _coefficient = entry.value;

            if (entry.sourceStat == null)
            {
                UnityEngine.Debug.LogWarning($"[ConvertStatBuffLogic] {buffInstance.data.buffID} 未配置 sourceStat，转化无效");
                return;
            }
            _sourceStat = entry.sourceStat.Value;

            // 使用 object.ReferenceEquals 绕过 Unity MonoBehaviour 的 fake-null，
            // 确保即使 owner 是已 Destroy 的 MonoBehaviour 包装也能正确访问 Stats 字段
            var stats = GetStats();
            if (stats != null)
                stats.OnStatChanged += OnSourceStatChanged;

            ApplyConversion();
        }

        public override void OnRemove()
        {
            var stats = GetStats();
            if (stats != null)
                stats.OnStatChanged -= OnSourceStatChanged;

            RemoveCurrentMod();
        }

        private void OnSourceStatChanged(StatType statType, float _)
        {
            if (statType == _sourceStat && !_applying)
                ApplyConversion();
        }

        private void ApplyConversion()
        {
            if (_applying) return;
            _applying = true;
            try
            {
                var stats = GetStats();
                if (stats == null) return;

                RemoveCurrentMod();

                float sourceValue = stats.GetStat(_sourceStat)?.FinalValue ?? 0f;
                float modValue    = sourceValue * _coefficient;

                _currentMod = new StatModifier(modValue, StatModType.Flat, buffInstance);
                stats.AddStatModifier(_targetStat, _currentMod);
            }
            finally
            {
                _applying = false;
            }
        }

        private void RemoveCurrentMod()
        {
            if (_currentMod == null) return;
            GetStats()?.RemoveStatModifier(_targetStat, _currentMod);
            _currentMod = null;
        }

        /// <summary>
        /// 绕过 Unity fake-null 获取 CharacterStats。
        /// MonoBehaviour 被 Destroy 后 C# 层引用仍有效，Stats 属性依然可读。
        /// </summary>
        private DreamSystem.Damage.Stat.CharacterStats GetStats()
        {
            if (object.ReferenceEquals(owner, null)) return null;
            return owner.Stats;
        }
    }
}

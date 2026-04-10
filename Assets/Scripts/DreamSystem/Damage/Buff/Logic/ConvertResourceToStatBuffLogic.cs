using DreamAttribute;
using DreamConfig;
using DreamSystem.Damage.Buff.Data;
using DreamSystem.Damage.Stat;
using Enum.Buff;

namespace DreamSystem.Damage.Buff.Logic
{
    /// <summary>
    /// 资源转化属性型 Buff 逻辑（跨界逻辑，目前为存根）。
    /// <para>
    /// 设计意图：根据玩家当前持有的金币量（外部经济系统）换算出攻击力加成。
    /// 例如：拥有 100 金币 × 系数 0.05 = 攻击力 +5。
    /// </para>
    /// <para>
    /// <b>当前限制</b>：<see cref="BuffBaseLogic"/> 只能访问 <c>IBuffOwner</c>
    /// （CharacterStats + GameObject），无法直接读取 <c>GameSessionData.CurrentCoinCount</c>。
    /// </para>
    /// <para>
    /// <b>后续方案</b>：在 <c>LevelManager</c> 或专用系统中监听金币变化事件，
    /// 调用 <c>ConvertResourceToStatBuffLogic.UpdateGoldCount(int gold)</c>
    /// 手动推送最新值，逻辑内部再更新修改器。
    /// </para>
    /// </summary>
    [BuffLogic(BuffLogicType.ConvertResourceToStat)]
    public class ConvertResourceToStatBuffLogic : BuffBaseLogic
    {
        private StatType _targetStat;
        private float _coefficient;
        private StatModifier _currentMod;

        public override void OnApply()
        {
            if (buffInstance.data is AttributeBuffData attrData && attrData.buffEntryDataList.Count > 0)
            {
                var entry = attrData.buffEntryDataList[0];
                _targetStat  = entry.statType;
                _coefficient = entry.value;
            }

            UnityEngine.Debug.LogWarning(
                "[ConvertResourceToStatBuffLogic] 此 Buff 需要外部系统推送金币量才能生效。" +
                "请在金币变化时调用 UpdateGoldCount()。效果暂未激活。");
        }

        public override void OnRemove()
        {
            RemoveCurrentMod();
        }

        /// <summary>
        /// 由外部系统（LevelManager / 金币事件监听）在金币数量变化时调用，
        /// 传入最新金币数以重新计算属性加成。
        /// </summary>
        public void UpdateGoldCount(int goldCount)
        {
            var stats = owner?.Stats;
            if (stats == null) return;

            RemoveCurrentMod();

            float modValue = goldCount * _coefficient;
            _currentMod = new StatModifier(modValue, StatModType.Flat, buffInstance);
            stats.AddStatModifier(_targetStat, _currentMod);
        }

        private void RemoveCurrentMod()
        {
            if (_currentMod == null) return;
            owner?.Stats?.RemoveStatModifier(_targetStat, _currentMod);
            _currentMod = null;
        }
    }
}

using DreamAttribute;
using DreamSystem.Damage.Buff.Data;
using DreamSystem.Damage.Stat;
using Enum.Buff;
namespace DreamSystem.Damage.Buff.Logic
{
    /// <summary>
    /// 属性修改型 Buff 逻辑。
    /// <para>生效时根据 AttributeBuffData 中的条目列表向目标 CharacterStats 添加修饰器，</para>
    /// <para>移除时通过 ClearSource 一键撤销所有修饰器。</para>
    /// </summary>
    [BuffLogic(BuffLogicType.ModifyAttribute)]
    public class ModifyAttributeBuffLogic : BuffBaseLogic
    {
        public override void OnApply()
        {
            var stats = owner?.Stats;
            if (stats == null) return;

            if (buffInstance.data is AttributeBuffData attrData)
            {
                foreach (var entry in attrData.buffEntryDataList)
                {
                    // 把 buffInstance 作为 source，方便 OnRemove 时用 ClearSource 批量移除
                    var mod = new StatModifier(entry.value, entry.modType, buffInstance);
                    stats.AddStatModifier(entry.statType, mod);
                }
            }
        }

        public override void OnRemove()
        {
            var stats = owner?.Stats;
            if (stats == null) return;

            stats.ClearSource(buffInstance);
        }
    }
}

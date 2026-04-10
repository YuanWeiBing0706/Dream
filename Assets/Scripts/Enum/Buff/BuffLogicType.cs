namespace Enum.Buff
{
    public enum BuffLogicType
    {
        /// 属性数值修改（Flat / PercentAdd / PercentMult）
        ModifyAttribute,

        /// 刺甲反弹（受击时对攻击方造成伤害）
        ThornsReflect,

        /// 属性转化（将 sourceStat 的百分比加到 targetStat 上，动态更新）
        ConvertStat,

        /// 控制锁定（禁用某个操作，如 Dash / Move / Attack）
        AddControlLock,

        /// 标记挂载（向 CharacterStats 添加字符串标记，用于外部系统读取）
        AddFlag,

        /// 资源转化为属性（根据当前金币等外部资源提供属性加成，需外部系统协作）
        ConvertResourceToStat,

        /// 事件触发型（订阅指定事件，条件满足时临时施加二级 Buff）
        TriggerOnEvent,
    }
}

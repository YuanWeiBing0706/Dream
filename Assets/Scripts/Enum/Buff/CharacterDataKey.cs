namespace Enum.Buff
{
    /// <summary>
    /// 角色数据变化 Key（用于 onDataChanged 精准刷新）。
    /// <para>StatType 枚举值作为属性变化 key，此枚举作为其他数据变化 key。</para>
    /// </summary>
    public enum CharacterDataKey
    {
        Flags = 1000,
        InventoryItems = 1001
    }
}
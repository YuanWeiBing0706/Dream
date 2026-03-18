using DreamSystem.Damage.Stat;
using UnityEngine;
namespace Interface
{
    /// <summary>
    /// Buff 系统持有者接口。
    /// <para>任何可以被施加 Buff 的实体（玩家、怪物、NPC 等）都应实现此接口。</para>
    /// <para>使 BuffSystem / BuffBaseLogic 与具体实体类型解耦。</para>
    /// </summary>
    public interface IBuffOwner
    {
        /// 实体对应的 GameObject（用于特效挂载、位置获取等）
        GameObject GameObject { get; }

        /// 实体的角色属性（用于 Buff 逻辑读写属性修改器）
        CharacterStats Stats { get; }
    }
}

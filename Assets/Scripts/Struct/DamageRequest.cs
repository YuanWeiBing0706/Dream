using DreamSystem.Damage;
using DreamSystem.Damage.Stat;
using UnityEngine;
namespace Struct
{
    /// <summary>
    /// 伤害请求数据包。
    /// <para>由 CombatSystem 发布，DamageSystem 接收处理。</para>
    /// </summary>
    public struct DamageRequest
    {
        /// 攻击方属性
        public CharacterStats AttackerStats;

        /// 受击方的 Collider（DamageSystem 通过它查表获取受击方 CharacterStats）
        public Collider TargetCollider;

        /// 基础伤害值（来自攻击配置）
        public float BaseDamage;

        public DamageRequest(CharacterStats attackerStats, Collider targetCollider, float baseDamage)
        {
            AttackerStats = attackerStats;
            TargetCollider = targetCollider;
            BaseDamage = baseDamage;
        }
    }
}
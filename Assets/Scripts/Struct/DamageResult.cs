using DreamSystem.Damage;
namespace Struct
{
    public struct DamageResult
    {
        /// 受击方属性（可读取当前血量等信息）
        public CharacterStats TargetStats;

        /// 结算后的最终伤害值
        public float FinalDamage;

        /// 受击方是否死亡
        public bool IsDead;

        public DamageResult(CharacterStats targetStats, float finalDamage, bool isDead)
        {
            TargetStats = targetStats;
            FinalDamage = finalDamage;
            IsDead = isDead;
        }
    }
}
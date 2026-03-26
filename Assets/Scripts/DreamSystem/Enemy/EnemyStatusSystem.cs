using UnityEngine;

namespace DreamSystem.Enemy
{
    public class EnemyStatusSystem : MonoBehaviour
    {
        /// <summary>
        /// 是否处于受击状态（由 EnemyInjuriedSystem 设置，供行为树条件节点读取）。
        /// </summary>
        public bool IsHit { get; private set; }

        /// <summary>
        /// 是否已死亡（由 EnemyInjuriedSystem 设置，供行为树条件节点读取）。
        /// </summary>
        public bool IsDead { get; private set; }

        public void SetHit(bool isHit)
        {
            IsHit = isHit;
        }

        public void SetDead(bool isDead)
        {
            IsDead = isDead;
        }
    }
}
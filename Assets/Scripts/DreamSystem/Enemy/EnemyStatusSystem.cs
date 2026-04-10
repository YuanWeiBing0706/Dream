using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DreamSystem.Enemy
{
    public class EnemyStatusSystem : MonoBehaviour
    {
        [Tooltip("死亡动画播放完毕后等待的时间（秒），然后回收 GO")]
        [SerializeField] private float DeathDelay = 2f;

        /// <summary>
        /// 是否处于受击状态（由 EnemyInjuriedSystem 设置，供行为树条件节点读取）。
        /// </summary>
        public bool IsHit { get; private set; }

        /// <summary>
        /// 是否已死亡（由 EnemyInjuriedSystem 设置，供行为树条件节点读取）。
        /// </summary>
        public bool IsDead { get; private set; }

        /// 从对象池重新激活时重置所有状态
        private void OnEnable()
        {
            IsDead = false;
            IsHit = false;
        }

        public void SetHit(bool isHit)
        {
            IsHit = isHit;
        }

        /// <summary>
        /// 标记死亡并在 DeathDelay 秒后将 GO 设为 inactive（回归对象池）。
        /// </summary>
        public void SetDead(bool isDead)
        {
            if (IsDead) return;
            IsDead = isDead;
            if (isDead)
                DeactivateAfterDelayAsync().Forget();
        }

        private async UniTaskVoid DeactivateAfterDelayAsync()
        {
            await UniTask.Delay(
                System.TimeSpan.FromSeconds(DeathDelay),
                cancellationToken: this.GetCancellationTokenOnDestroy());
            gameObject.SetActive(false);
        }
    }
}

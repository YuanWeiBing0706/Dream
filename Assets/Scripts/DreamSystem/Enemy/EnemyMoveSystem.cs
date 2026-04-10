using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace DreamSystem.Enemy
{
    public class EnemyMoveSystem : MonoBehaviour
    {
        /// NavMeshAgent 寻路组件
        [SerializeField]
        private NavMeshAgent NavMeshAgent;

        /// 敌人动画系统（纯被动播放器）
        [SerializeField]
        private EnemyAnimationSystem AnimationSystem;
        
        /// 玩家对象缓存（懒加载）
        private GameObject _playerGameObject;

        /// 懒加载获取玩家对象，避免因生成顺序导致永久 null
        private GameObject PlayerGameObject
        {
            get
            {
                if (_playerGameObject == null)
                {
                    _playerGameObject = GameObject.FindWithTag("Player");
                }
                return _playerGameObject;
            }
        }

        /// 攻击范围（与行为树 IsPlayerInRange 配合使用）
        [SerializeField]
        private float AttackRange;

        /// 待机缓冲范围（用于防止打完以后疯狂鬼畜推人，通常设为 1.5 - 2.0 左右）
        [SerializeField]
        private float WaitRangeBuf = 1.5f;

        /// 移动速度
        [SerializeField]
        private float Speed;

        /// <summary>
        /// 从对象池重新激活时，异步强制将 NavMeshAgent 传送到正确出生点。
        /// <para>NavMesh 系统在同帧内不会完成 Agent 的重新放置（isOnNavMesh 仍为 false），
        /// 必须等一帧让引擎完成初始化后再 Warp，否则 Agent 会用旧位置导致卡墙。</para>
        /// </summary>
        private void OnEnable()
        {
            if (NavMeshAgent == null) return;
            _playerGameObject = null; // 清除玩家缓存，避免引用上一局已销毁的玩家

            // 不能 disable NavMeshAgent：行为树在同帧就会调用 SetDestination，
            // 若 Agent 被禁用则静默失败，行为树进入"等待寻路"状态不再重试，
            // 导致怪物卡在出生点直到下一次状态机重新评估。
            //
            // NavMeshAgent 自身的 OnEnable 会在本组件的 OnEnable 之前运行，
            // 它已将 Agent 放置到 transform.position（PoolManager 出生点），
            // 我们只需清除旧路径，并在同帧或下一帧 Warp 到精确采样点即可。

            NavMeshAgent.ResetPath(); // 清除上一局残留路径

            if (NavMeshAgent.isOnNavMesh)
            {
                WarpToSpawnPosition();
            }
            else
            {
                // 极少数情况：NavMesh 尚未在本帧完成绑定，保持 Agent 启用，
                // 下一帧再 Warp（行为树此时也只刚开始运行，不影响指令）
                DeferredWarpAsync().Forget();
            }
        }

        private void WarpToSpawnPosition()
        {
            Vector3 target = transform.position;
            if (NavMesh.SamplePosition(target, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                target = hit.position;
            NavMeshAgent.Warp(target);
        }

        private async UniTaskVoid DeferredWarpAsync()
        {
            var ct = this.GetCancellationTokenOnDestroy();
            await UniTask.NextFrame(ct);
            if (ct.IsCancellationRequested || NavMeshAgent == null) return;
            WarpToSpawnPosition();
        }

        /// 追击目标：设置 NavMesh 目的地为目标位置。
        /// </summary>
        /// <param name="target">追击目标的 Transform</param>
        public void ChaseTarget(Transform target)
        {
            NavMeshAgent.SetDestination(target.position);
        }

        /// <summary>
        /// 停止移动：清除当前寻路路径并播放待机动画。
        /// </summary>
        public void Stop()
        {
            NavMeshAgent.ResetPath();
            if (AnimationSystem != null) AnimationSystem.PlayIdle();
        }

        /// <summary>
        /// 巡逻到指定目标点。
        /// </summary>
        /// <param name="point">世界坐标下的目标点</param>
        public void PatrolTo(Vector3 point)
        {
            NavMeshAgent.SetDestination(point);
        }

        /// <summary>
        /// 判断与目标的距离是否在指定范围内。
        /// </summary>
        /// <param name="target">目标 Transform</param>
        /// <param name="range">判定范围</param>
        /// <returns>是否在范围内</returns>
        public bool IsInRange(Transform target, float range)
        {
            return Vector3.Distance(transform.position, target.position) < range;
        }

        /// <summary>
        /// 判断玩家是否在攻击范围内（供行为树无参反射调用，避免预制体传参问题）。
        /// </summary>
        /// <returns>玩家是否在 AttackRange 内</returns>
        public bool IsPlayerInRange()
        {
            if (PlayerGameObject == null) return false;
            return Vector3.Distance(transform.position, PlayerGameObject.transform.position) < AttackRange;
        }

        /// <summary>
        /// 新增的无参版本：判断玩家是否在【脱战发呆缓冲范围】内。
        /// 给追踪加一个“缓冲带”，防止怪物一直在边界上疯狂推人。
        /// </summary>
        public bool IsPlayerInWaitRange()
        {
            if (PlayerGameObject == null) return false;
            return Vector3.Distance(transform.position, PlayerGameObject.transform.position) < (AttackRange + WaitRangeBuf);
        }

        /// <summary>
        /// 追击玩家（供行为树无参反射调用）。
        /// </summary>
        public void ChasePlayer()
        {
            if (PlayerGameObject == null) return;
            NavMeshAgent.SetDestination(PlayerGameObject.transform.position);
            if (AnimationSystem != null) AnimationSystem.PlayMove();
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace DreamSystem.Enemy
{
    /// <summary>
    /// 敌人移动系统。
    /// <para>挂在敌人预制体上，供行为树调用。</para>
    /// <para>职责：基于 NavMeshAgent 的寻路移动、距离判断、巡逻控制。</para>
    /// </summary>
    public class EnemyMoveSystem : MonoBehaviour
    {
        [SerializeField]
        private NavMeshAgent NavMeshAgent;
        
        private GameObject _playerGameObject;

        /// 攻击范围（与行为树 IsInRange 配合使用）
        [SerializeField]
        private float AttackRange;

        /// 移动速度
        [SerializeField]
        private float Speed;
        
        public void Start()
        {
            _playerGameObject = GameObject.FindWithTag("Player");
        }

        /// <summary>
        /// 追击目标：设置 NavMesh 目的地为目标位置。
        /// </summary>
        /// <param name="target">追击目标的 Transform</param>
        public void ChaseTarget(Transform target)
        {
            NavMeshAgent.SetDestination(target.position);
        }

        /// <summary>
        /// 停止移动：清除当前寻路路径。
        /// </summary>
        public void Stop()
        {
            NavMeshAgent.ResetPath();
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
        /// 判断与目标的距离是否在指定范围内（行为树 Condition 节点调用）。
        /// </summary>
        /// <param name="target">目标 Transform</param>
        /// <param name="range">判定范围</param>
        /// <returns>是否在范围内</returns>
        public bool IsInRange(Transform target, float range)
        {
            return Vector3.Distance(transform.position, target.position) < range;
        }
    }
}

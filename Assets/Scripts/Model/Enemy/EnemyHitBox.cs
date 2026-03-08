using UnityEngine;

namespace Model.Enemy
{
    /// <summary>
    /// 敌人攻击碰撞检测盒。
    /// <para>挂在敌人预制体上，用于检测攻击命中的目标。</para>
    /// <para>结构与 PlayerHitBox 一致，检测目标为 Player 层。</para>
    /// </summary>
    public class EnemyHitBox : MonoBehaviour
    {
        /// 检测盒大小
        public Vector3 BoxSize = Vector3.one;

        /// 检测盒偏移
        public Vector3 Offset = Vector3.zero;

        /// 目标检测层（应设为 Player 层）
        public LayerMask HitLayerArr;

        /// 检测结果缓存（预分配，避免 GC）
        private Collider[] _hitResults = new Collider[5];

        /// <summary>
        /// 执行检测，返回检测到的物体数量和数组。
        /// </summary>
        public int Detect(out Collider[] results)
        {
            Vector3 center = transform.TransformPoint(Offset);
            int count = Physics.OverlapBoxNonAlloc(center, BoxSize / 2, _hitResults, transform.rotation, HitLayerArr);

            results = _hitResults;
            return count;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1, 0.5f, 0, 0.4f);

            Matrix4x4 rotationMatrix = Matrix4x4.TRS(
                transform.TransformPoint(Offset),
                transform.rotation,
                Vector3.one
            );

            Gizmos.matrix = rotationMatrix;

            Gizmos.DrawCube(Vector3.zero, BoxSize);
            Gizmos.DrawWireCube(Vector3.zero, BoxSize);
        }
    }
}

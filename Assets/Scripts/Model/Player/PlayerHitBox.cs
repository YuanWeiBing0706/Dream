using UnityEngine;
using UnityEngine.Serialization;

namespace Model.Player
{
    public class PlayerHitBox : MonoBehaviour
    {
        public Vector3 BoxSize = Vector3.one;
        public Vector3 Offset = Vector3.zero;
        public LayerMask HitLayerArr;
        
        private Collider[] _hitResults = new Collider[10];

        /// <summary>
        /// 执行检测，返回检测到的物体数量和数组
        /// </summary>
        public int Detect(out Collider[] results)
        {
            // 计算世界坐标中心点
            Vector3 center = transform.TransformPoint(Offset);
            // 物理检测
            int count = Physics.OverlapBoxNonAlloc(center, BoxSize / 2, _hitResults, transform.rotation, HitLayerArr);
            
            results = _hitResults;
            return count;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1, 0, 0, 0.4f);
            
            // 矩阵变换：处理旋转和位移
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
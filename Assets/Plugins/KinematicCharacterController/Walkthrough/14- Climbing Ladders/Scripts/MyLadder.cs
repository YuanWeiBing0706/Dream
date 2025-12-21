using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController.Walkthrough.ClimbingLadders
{
    public class MyLadder : MonoBehaviour
    {
        // 梯子段
        public Vector3 LadderSegmentBottom;
        public float LadderSegmentLength;

        // 到达梯子一端并离开梯子时要移动到的点
        public Transform BottomReleasePoint;
        public Transform TopReleasePoint;

        // 获取梯子段底部点的位置
        public Vector3 BottomAnchorPoint
        {
            get
            {
                return transform.position + transform.TransformVector(LadderSegmentBottom);
            }
        }

        // 获取梯子段顶部点的位置
        public Vector3 TopAnchorPoint
        {
            get
            {
                return transform.position + transform.TransformVector(LadderSegmentBottom) + (transform.up * LadderSegmentLength);
            }
        }

        public Vector3 ClosestPointOnLadderSegment(Vector3 fromPoint, out float onSegmentState)
        {
            Vector3 segment = TopAnchorPoint - BottomAnchorPoint;            
            Vector3 segmentPoint1ToPoint = fromPoint - BottomAnchorPoint;
            float pointProjectionLength = Vector3.Dot(segmentPoint1ToPoint, segment.normalized);

            // 当高于底部点时
            if(pointProjectionLength > 0)
            {
                // 如果我们不高于顶部点
                if (pointProjectionLength <= segment.magnitude)
                {
                    onSegmentState = 0;
                    return BottomAnchorPoint + (segment.normalized * pointProjectionLength);
                }
                // 如果我们高于顶部点
                else
                {
                    onSegmentState = pointProjectionLength - segment.magnitude;
                    return TopAnchorPoint;
                }
            }
            // 当低于底部点时
            else
            {
                onSegmentState = pointProjectionLength;
                return BottomAnchorPoint;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(BottomAnchorPoint, TopAnchorPoint);
        }
    }
}
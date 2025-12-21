using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

namespace KinematicCharacterController.Walkthrough.RootMotionExample
{
    public struct PlayerCharacterInputs
    {
        public float MoveAxisForward;
        public float MoveAxisRight;
    }

    public class MyCharacterController : MonoBehaviour, ICharacterController
    {
        public KinematicCharacterMotor Motor;

        [Header("Stable Movement")]
        public float MaxStableMoveSpeed = 10f;
        public float StableMovementSharpness = 15;
        public float OrientationSharpness = 10;

        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 10f;
        public float AirAccelerationSpeed = 5f;
        public float Drag = 0.1f;

        [Header("Animation Parameters")]
        public Animator CharacterAnimator;
        public float ForwardAxisSharpness = 10;
        public float TurnAxisSharpness = 5;

        [Header("Misc")]
        public Vector3 Gravity = new Vector3(0, -30f, 0);
        public Transform MeshRoot;

        private Vector3 _moveInputVector;
        private Vector3 _lookInputVector;
        private float _forwardAxis;
        private float _rightAxis;
        private float _targetForwardAxis;
        private float _targetRightAxis;
        private Vector3 _rootMotionPositionDelta;
        private Quaternion _rootMotionRotationDelta;

        /// <summary>
        /// 由MyPlayer每帧调用，以告诉角色其输入是什么
        /// </summary>
        public void SetInputs(ref PlayerCharacterInputs inputs)
        {
            // 轴输入
            _targetForwardAxis = inputs.MoveAxisForward;
            _targetRightAxis = inputs.MoveAxisRight;
        }

        private void Start()
        {
            _rootMotionPositionDelta = Vector3.zero;
            _rootMotionRotationDelta = Quaternion.identity;

            // 分配给电机
            Motor.CharacterController = this;
        }

        private void Update()
        {
            // 处理动画
            _forwardAxis = Mathf.Lerp(_forwardAxis, _targetForwardAxis, 1f - Mathf.Exp(-ForwardAxisSharpness * Time.deltaTime));
            _rightAxis = Mathf.Lerp(_rightAxis, _targetRightAxis, 1f - Mathf.Exp(-TurnAxisSharpness * Time.deltaTime));
            CharacterAnimator.SetFloat("Forward", _forwardAxis);
            CharacterAnimator.SetFloat("Turn", _rightAxis);
            CharacterAnimator.SetBool("OnGround", Motor.GroundingStatus.IsStableOnGround);
        }

        /// <summary>
        /// （由KinematicCharacterMotor在其更新周期中调用）
        /// 在角色开始其移动更新之前调用
        /// </summary>
        public void BeforeCharacterUpdate(float deltaTime)
        {
        }

        /// <summary>
        /// （由KinematicCharacterMotor在其更新周期中调用）
        /// 在这里告诉角色其旋转应该是什么。这是您应该设置角色旋转的唯一位置
        /// </summary>
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            currentRotation = _rootMotionRotationDelta * currentRotation;
        }

        /// <summary>
        /// （由KinematicCharacterMotor在其更新周期中调用）
        /// 在这里告诉角色其速度应该是什么。这是您可以设置角色速度的唯一位置
        /// </summary>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (Motor.GroundingStatus.IsStableOnGround)
            {
                if (deltaTime > 0)
                {
                    // 最终速度是来自根运动的速度，在地面平面上重新定向
                    currentVelocity = _rootMotionPositionDelta / deltaTime;
                    currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;
                }
                else
                {
                    // 防止除以零
                    currentVelocity = Vector3.zero;
                }
            }
            else
            {
                if (_forwardAxis > 0f)
                {
                    // 如果我们想移动，向速度添加加速度
                    Vector3 targetMovementVelocity = Motor.CharacterForward * _forwardAxis * MaxAirMoveSpeed;
                    Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
                    currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime;
                }

                // 重力
                currentVelocity += Gravity * deltaTime;

                // 阻力
                currentVelocity *= (1f / (1f + (Drag * deltaTime)));
            }
        }

        /// <summary>
        /// （由KinematicCharacterMotor在其更新周期中调用）
        /// 在角色完成其移动更新后调用
        /// </summary>
        public void AfterCharacterUpdate(float deltaTime)
        {
            // 重置根运动增量
            _rootMotionPositionDelta = Vector3.zero;
            _rootMotionRotationDelta = Quaternion.identity;
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void PostGroundingUpdate(float deltaTime)
        {
        }

        public void AddVelocity(Vector3 velocity)
        {
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        private void OnAnimatorMove()
        {
            // 在角色更新之间累积根运动增量
            _rootMotionPositionDelta += CharacterAnimator.deltaPosition;
            _rootMotionRotationDelta = CharacterAnimator.deltaRotation * _rootMotionRotationDelta;
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }
    }
}
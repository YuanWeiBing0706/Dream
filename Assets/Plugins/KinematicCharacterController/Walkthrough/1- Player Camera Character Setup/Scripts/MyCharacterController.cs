using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

namespace KinematicCharacterController.Walkthrough.PlayerCameraCharacterSetup
{
    public class MyCharacterController : MonoBehaviour, ICharacterController
    {
        public KinematicCharacterMotor Motor;

        private void Start()
        {
            // 绑定到角色电机
            Motor.CharacterController = this;
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
            // 在角色电机执行任何逻辑之前被调用
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            // 当角色电机需要知道此刻应采用的旋转时被调用
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            // 当角色电机需要知道此刻应采用的速度时被调用
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            // 在角色电机完成本帧所有更新后被调用
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            // 当角色电机想知道某个碰撞体是否应该参与碰撞（还是可以穿过）时被调用
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            // 当角色电机的地面探测检测到与地面的接触时被调用
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            // 当角色电机的移动逻辑检测到碰撞时被调用
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
            // 在角色电机检测到每一次碰撞之后被调用，允许你按需修改 HitStabilityReport
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            // 在角色电机完成地面探测之后、处理 PhysicsMover / 速度 等逻辑之前被调用
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
            // 当角色电机检测到一种不是由“移动碰撞”产生的碰撞时被调用
        }
    }
}
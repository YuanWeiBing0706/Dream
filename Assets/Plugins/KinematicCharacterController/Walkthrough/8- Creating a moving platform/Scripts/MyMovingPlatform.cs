using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Playables;

namespace KinematicCharacterController.Walkthrough.MovingPlatform
{
    public struct MyMovingPlatformState
    {
        public PhysicsMoverState MoverState;
        public float DirectorTime;
    }

    public class MyMovingPlatform : MonoBehaviour, IMoverController
    {
        public PhysicsMover Mover;

        public PlayableDirector Director;

        private Transform _transform;

        private void Start()
        {
            _transform = this.transform;

            Mover.MoverController = this;
        }

        // 这由我们的PhysicsMover在每个FixedUpdate中调用，以告诉它应该到达什么姿态
        public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            // 记住动画前的姿态
            Vector3 _positionBeforeAnim = _transform.position;
            Quaternion _rotationBeforeAnim = _transform.rotation;

            // 更新动画
            EvaluateAtTime(Time.time);

            // 将我们的平台目标姿态设置为动画的姿态
            goalPosition = _transform.position;
            goalRotation = _transform.rotation;

            // 将实际变换姿态重置为评估前的状态。
            // 这样真正的移动可以由物理移动器处理；而不是动画
            _transform.position = _positionBeforeAnim;
            _transform.rotation = _rotationBeforeAnim;
        }

        public void EvaluateAtTime(double time)
        {
            Director.time = time % Director.duration;
            Director.Evaluate();
        }
    }
}
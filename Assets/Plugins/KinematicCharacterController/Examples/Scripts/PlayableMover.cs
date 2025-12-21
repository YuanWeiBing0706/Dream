using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Playables;

namespace KinematicCharacterController.Examples
{
    public class PlayableMover : MonoBehaviour, IMoverController
    {
        public PhysicsMover Mover;

        public float Speed = 1f;
        public PlayableDirector Director;

        private Transform _transform;

        private void Start()
        {
            _transform = this.transform;
            Director.timeUpdateMode = DirectorUpdateMode.Manual;

            Mover.MoverController = this;
        }

        // 这个方法会在每次 FixedUpdate 时由 PhysicsMover 调用，用来告知平台应移动到的目标姿态
        public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            // 记录播放动画前的平台姿态
            Vector3 _positionBeforeAnim = _transform.position;
            Quaternion _rotationBeforeAnim = _transform.rotation;

            // 更新动画
            EvaluateAtTime(Time.time * Speed);

            // 将平台的目标姿态设置为动画计算出的姿态
            goalPosition = _transform.position;
            goalRotation = _transform.rotation;

            // 将实际 Transform 姿态重置到评估动画前的状态
            // 这样真正的运动就由 PhysicsMover 来处理，而不是直接由动画驱动
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
using System;
using Events;
using Interface.IUntiy;
using UnityEngine;
namespace Dream
{
    [GameSystem(CollectType.Auto)]
    public class PlayerAngleViewSystem : GameSystem, ILateUpdate
    {
        private GameObject _mainCamera;

        ///目标玩家
        private GameObject _targetPlayer;

        public float topClamp = 70.0f;

        public float bottomClamp = -30.0f;

        private const float THRESHOLD = 0.01f;
        private float _cinemaChineTargetYaw;
        private float _cinemaChineTargetPitch;

        private Vector2 _look;
        public override void Init()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

            if (_targetPlayer == null)
            {
                _targetPlayer = GameObject.FindGameObjectWithTag("Player");
            }

            if (_targetPlayer != null)
            {
                var euler = _targetPlayer.transform.rotation.eulerAngles;
                _cinemaChineTargetYaw = euler.y;
                _cinemaChineTargetPitch = euler.x;
            }

            EventManager.Instance.Subscribe<Vector2>(GameEvents.PLAYER_ANGLE_VIEW_PERFORMED, PlayerMoveAngleViewPerformed);
            // 注册到全局 LateUpdate 调度
            EventManager.Instance.Publish<ILateUpdate>(GameEvents.LATE_UPDATE_REGISTER, this);
        }
        private void PlayerMoveAngleViewPerformed(Vector2 obj)
        {
            _look = obj;
        }

        public override void ManualDispose()
        {
            EventManager.Instance.Unsubscribe<Vector2>(GameEvents.PLAYER_ANGLE_VIEW_PERFORMED, PlayerMoveAngleViewPerformed);
        }

        public void LateUpdate()
        {
            if (_targetPlayer == null)
            {
                return;
            }
            if (_look.sqrMagnitude > THRESHOLD)
            {
                _cinemaChineTargetYaw += _look.x;
                _cinemaChineTargetPitch += _look.y;
            }


            _cinemaChineTargetYaw = ClampAngle(_cinemaChineTargetYaw, float.MinValue, float.MaxValue);
            _cinemaChineTargetPitch = ClampAngle(_cinemaChineTargetPitch, bottomClamp, topClamp);

            _targetPlayer.transform.rotation = Quaternion.Euler(_cinemaChineTargetPitch, _cinemaChineTargetYaw, 0.0f);
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

    }
}
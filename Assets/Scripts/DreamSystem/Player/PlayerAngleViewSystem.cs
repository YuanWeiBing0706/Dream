using Dream;
using Model.Player;
using UnityEngine;
namespace DreamSystem.Player
{
    public class PlayerAngleViewSystem : GameSystem
    {
        private readonly EventManager _eventManager;
        private readonly PlayerModel _playerModel;

        private GameObject _targetPlayer;
        private Vector2 _look;
        private float _cinemaChineTargetYaw;
        private float _cinemaChineTargetPitch;

        public float topClamp = 70.0f;
        public float bottomClamp = -30.0f;
        private const float THRESHOLD = 0.01f;

        public PlayerAngleViewSystem(EventManager eventManager, PlayerModel playerModel)
        {
            _eventManager = eventManager;
            _playerModel = playerModel;
        }

        public override void Start()
        {
            _targetPlayer = _playerModel.gameObject;
            var euler = _targetPlayer.transform.rotation.eulerAngles;
            _cinemaChineTargetYaw = euler.y;
            _cinemaChineTargetPitch = euler.x;
        }

        public override void LateTick()
        {
            if (_look.sqrMagnitude > THRESHOLD)
            {
                _cinemaChineTargetYaw += _look.x;
                _cinemaChineTargetPitch += _look.y;
            }

            _cinemaChineTargetYaw = ClampAngle(_cinemaChineTargetYaw, float.MinValue, float.MaxValue);
            _cinemaChineTargetPitch = ClampAngle(_cinemaChineTargetPitch, bottomClamp, topClamp);

            _targetPlayer.transform.rotation = Quaternion.Euler(_cinemaChineTargetPitch, _cinemaChineTargetYaw, 0.0f);
        }

        private void PlayerMoveAngleViewPerformed(Vector2 obj)
        {
            _look = obj;
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f)
            {
                lfAngle += 360f;
            }
            if (lfAngle > 360f)
            {
                lfAngle -= 360f;
            }
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        public override void Dispose()
        {
            _eventManager.Unsubscribe<Vector2>(Events.GameEvents.PLAYER_ANGLE_VIEW_PERFORMED, PlayerMoveAngleViewPerformed);
        }
    }
}
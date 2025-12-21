using Cysharp.Threading.Tasks;
using DreamManager;
using Events;
using Model.Player;
using Struct;
using UnityEngine;

namespace DreamSystem.Player
{
    public class PlayerMoveSystem : GameSystem
    {
        // --- 基础配置 ---
        public float moveSpeed = 6f;

        // --- 内部状态 ---
        private Vector2 _moveInput;
        private bool _isJumpDown;

        // --- 依赖引用 ---
        private readonly EventManager _eventManager;
        private readonly KccMoveController _kccController;
        private Transform _cameraTransform;

        public PlayerMoveSystem(EventManager eventManager, KccMoveController kccController, Transform cameraTransform)
        {
            _eventManager = eventManager;
            _kccController = kccController;
            _cameraTransform = cameraTransform;
        }

        public override void Start()
        {
            _eventManager.Subscribe<Vector2>(GameEvents.PLAYER_MOVE_PERFORMED, OnMovePerformed);
            _eventManager.Subscribe<Vector2>(GameEvents.PLAYER_MOVE_CANCELED, OnMoveCanceled);
            _eventManager.Subscribe<bool>(GameEvents.PLAYER_JUMP_PERFROMED, OnJumpPerformed);
        }


        private void OnMovePerformed(Vector2 input) => _moveInput = input;
        private void OnMoveCanceled(Vector2 input) => _moveInput = Vector2.zero;

        private void OnJumpPerformed(bool isJumpDown) => _isJumpDown = isJumpDown;

        private void OnJumpCanceled(bool isJumpDown) => _isJumpDown = isJumpDown;


        public override void LateTick()
        {
            if (_kccController == null) return;

            // 1. 计算世界坐标下的移动向量
            Vector3 worldMoveVelocity = CalculateMoveVelocity();

            KccInputs inputsPacket = new KccInputs
            {
                moveDirection = worldMoveVelocity.normalized, //计算朝向 (通常就是移动方向，或者是锁定的目标方向)
                cameraRotation = _cameraTransform.rotation,
                jumpDown = _isJumpDown // TODO: 连接你的 InputSystem
            };

            _eventManager.Publish(GameEvents.SET_INPUTS, inputsPacket);
        }

        private Vector3 CalculateMoveVelocity()
        {
            if (_moveInput.sqrMagnitude < 0.01f) return Vector3.zero;

            Vector3 camForward = _cameraTransform.forward;
            Vector3 camRight = _cameraTransform.right;

            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            // 方向 * 速度
            Vector3 targetDirection = (camForward * _moveInput.y + camRight * _moveInput.x).normalized;
            return targetDirection * moveSpeed;
        }

        public override void Dispose()
        {
            _eventManager.Unsubscribe<Vector2>(GameEvents.PLAYER_MOVE_PERFORMED, OnMovePerformed).Forget();
            _eventManager.Unsubscribe<Vector2>(GameEvents.PLAYER_MOVE_CANCELED, OnMoveCanceled).Forget();
            _eventManager.Unsubscribe<bool>(GameEvents.PLAYER_JUMP_PERFROMED, OnJumpPerformed).Forget();
            _eventManager.Unsubscribe<bool>(GameEvents.PLAYER_JUMP_CANCELED, OnJumpCanceled).Forget();
        }


    }
}
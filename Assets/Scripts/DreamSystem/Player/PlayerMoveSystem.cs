using Const;
using Cysharp.Threading.Tasks;
using DreamManager;
using Struct;
using UnityEngine;

namespace DreamSystem.Player
{
    /// <summary>
    /// 玩家移动输入系统。
    /// <para>职责：接收原始输入事件、计算相机相对方向、组装 MoveInputs 并发布给 KccMoveController。</para>
    /// </summary>
    public class PlayerMoveSystem : GameSystem
    {
        /// 玩家基础移动速度
        public float moveSpeed = 6f;

        /// 当前帧移动输入
        private Vector2 _moveInput;

        /// 当前帧跳跃输入
        private bool _isJumpDown;

        /// 当前帧闪避输入
        private bool _isDodgeDown;

        /// 事件管理器
        private readonly EventManager _eventManager;

        /// KCC 移动控制器引用
        private readonly KccMoveController _kccController;

        /// 相机 Transform (用于计算相对方向)
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
            _eventManager.Subscribe<bool>(GameEvents.PLAYER_JUMP_CANCELED, OnJumpCanceled);
            _eventManager.Subscribe<bool>(GameEvents.PLAYER_DODGE_PERFORMED, OnDodgePerformed);
            _eventManager.Subscribe<bool>(GameEvents.PLAYER_DODGE_CANCELED, OnDodgeCanceled);
        }


        // ===== 输入事件回调 =====
        private void OnMovePerformed(Vector2 input) => _moveInput = input;
        private void OnMoveCanceled(Vector2 input) => _moveInput = Vector2.zero;
        private void OnJumpPerformed(bool isJumpDown) => _isJumpDown = isJumpDown;
        private void OnJumpCanceled(bool isJumpDown) => _isJumpDown = isJumpDown;
        private void OnDodgePerformed(bool isDodgeDown) => _isDodgeDown = isDodgeDown;
        private void OnDodgeCanceled(bool isDodgeDown) => _isDodgeDown = isDodgeDown;
        
        /// <summary>
        /// 每帧后期更新：组装输入数据并发布。
        /// </summary>
        public override void LateTick()
        {
            if (_kccController == null) return;
            if (_cameraTransform == null) return;

            // 计算世界坐标下的移动方向
            Vector3 worldMoveVelocity = CalculateMoveVelocity();

            // 组装输入数据包
            MoveInputs inputsPacket = new MoveInputs
            {
                moveDirection = worldMoveVelocity.normalized,
                cameraRotation = _cameraTransform.rotation,
                jumpDown = _isJumpDown,
                isDodge = _isDodgeDown
            };

            // 重置单帧输入
            _isJumpDown = false;
            _isDodgeDown = false;

            _eventManager.Publish(GameEvents.SET_INPUTS, inputsPacket);
        }

        /// <summary>
        /// 根据输入计算相对于相机的移动向量。
        /// </summary>
        /// <returns>世界坐标下的移动方向向量</returns>
        private Vector3 CalculateMoveVelocity()
        {
            if (_moveInput.sqrMagnitude < 0.01f) return Vector3.zero;

            // 获取相机前后左右方向
            Vector3 camForward = _cameraTransform.forward;
            Vector3 camRight = _cameraTransform.right;

            // 投影到水平面
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            // 计算移动方向
            Vector3 targetDirection = (camForward * _moveInput.y + camRight * _moveInput.x).normalized;
            return targetDirection * moveSpeed;
        }

        /// <summary>
        /// 释放资源，取消事件订阅。
        /// </summary>
        public override void Dispose()
        {
            _eventManager.Unsubscribe<Vector2>(GameEvents.PLAYER_MOVE_PERFORMED, OnMovePerformed);
            _eventManager.Unsubscribe<Vector2>(GameEvents.PLAYER_MOVE_CANCELED, OnMoveCanceled);
            _eventManager.Unsubscribe<bool>(GameEvents.PLAYER_JUMP_PERFROMED, OnJumpPerformed);
            _eventManager.Unsubscribe<bool>(GameEvents.PLAYER_JUMP_CANCELED, OnJumpCanceled);
            _eventManager.Unsubscribe<bool>(GameEvents.PLAYER_DODGE_PERFORMED, OnDodgePerformed);
            _eventManager.Unsubscribe<bool>(GameEvents.PLAYER_DODGE_CANCELED, OnDodgeCanceled);
        }
    }
}
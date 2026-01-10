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

        /// 玩家移动的基础速度
        public float moveSpeed = 6f;
        
        /// 当前帧的移动输入向量
        private Vector2 _moveInput;
        /// 当前是否按下了跳跃键
        private bool _isJumpDown;
        
        /// 事件管理器引用
        private readonly EventManager _eventManager;
        /// KCC 移动控制器引用
        private readonly KccMoveController _kccController;
        /// 相机变换组件引用
        private Transform _cameraTransform;

        /// <summary>
        /// 构造函数，注入依赖项
        /// </summary>
        /// <param name="eventManager">事件管理器实例</param>
        /// <param name="kccController">KCC 移动控制器实例</param>
        /// <param name="cameraTransform">主相机 Transform</param>
        public PlayerMoveSystem(EventManager eventManager, KccMoveController kccController, Transform cameraTransform)
        {
            _eventManager = eventManager;
            _kccController = kccController;
            _cameraTransform = cameraTransform;
        }

        /// <summary>
        /// 系统启动时的初始化逻辑
        /// </summary>
        public override void Start()
        {
            _eventManager.Subscribe<Vector2>(GameEvents.PLAYER_MOVE_PERFORMED, OnMovePerformed);
            _eventManager.Subscribe<Vector2>(GameEvents.PLAYER_MOVE_CANCELED, OnMoveCanceled);
            _eventManager.Subscribe<bool>(GameEvents.PLAYER_JUMP_PERFROMED, OnJumpPerformed);
        }

        /// <summary>
        /// 当接收到移动输入时的回调
        /// </summary>
        /// <param name="input">输入的二维向量</param>
        private void OnMovePerformed(Vector2 input) => _moveInput = input;

        /// <summary>
        /// 当移动输入取消时的回调
        /// </summary>
        /// <param name="input">输入的二维向量（通常为零）</param>
        private void OnMoveCanceled(Vector2 input) => _moveInput = Vector2.zero;

        /// <summary>
        /// 当接收到跳跃输入时的回调
        /// </summary>
        /// <param name="isJumpDown">是否按下跳跃键</param>
        private void OnJumpPerformed(bool isJumpDown) => _isJumpDown = isJumpDown;

        /// <summary>
        /// 当跳跃输入取消时的回调
        /// </summary>
        /// <param name="isJumpDown">是否按下跳跃键</param>
        private void OnJumpCanceled(bool isJumpDown) => _isJumpDown = isJumpDown;
        
        /// <summary>
        /// 每一帧的后期更新逻辑，处理输入数据包的组装与发送
        /// </summary>
        public override void LateTick()
        {
            if (_kccController == null) return;

            // 计算世界坐标下的移动向量
            Vector3 worldMoveVelocity = CalculateMoveVelocity();

            KccInputs inputsPacket = new KccInputs
            {
                moveDirection = worldMoveVelocity.normalized, // 计算朝向 (通常就是移动方向，或者是锁定的目标方向)
                cameraRotation = _cameraTransform.rotation,
                jumpDown = _isJumpDown // TODO: 此处尚未完全对接 Unity Input System 的 Jump Action，需后续确认按键映射
            };

            _eventManager.Publish(GameEvents.SET_INPUTS, inputsPacket);
        }

        /// <summary>
        /// 根据输入计算相对于相机的移动向量
        /// </summary>
        /// <returns>计算后的世界坐标移动向量</returns>
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

        /// <summary>
        /// 释放资源，取消事件订阅
        /// </summary>
        public override void Dispose()
        {
            _eventManager.Unsubscribe<Vector2>(GameEvents.PLAYER_MOVE_PERFORMED, OnMovePerformed).Forget();
            _eventManager.Unsubscribe<Vector2>(GameEvents.PLAYER_MOVE_CANCELED, OnMoveCanceled).Forget();
            _eventManager.Unsubscribe<bool>(GameEvents.PLAYER_JUMP_PERFROMED, OnJumpPerformed).Forget();
            _eventManager.Unsubscribe<bool>(GameEvents.PLAYER_JUMP_CANCELED, OnJumpCanceled).Forget();
        }


    }
}
using DreamManager;
using Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DreamSystem
{
    public class PlayerInputSystem : GameSystem
    {
        /// 输入管理器引用
        private readonly GameInputManager _gameInputManager;
        /// 事件管理器引用
        private readonly EventManager _eventManager;

        /// <summary>
        /// 构造函数，注入依赖项
        /// </summary>
        /// <param name="inputManager">游戏输入管理器</param>
        /// <param name="eventManager">事件管理器</param>
        public PlayerInputSystem(GameInputManager inputManager, EventManager eventManager)
        {
            _gameInputManager = inputManager;
            _eventManager = eventManager;
        }

        /// <summary>
        /// 系统启动时的初始化逻辑
        /// </summary>
        public override void Start()
        {
            // 锁定在屏幕中心
            Cursor.lockState = CursorLockMode.Locked;
            // 隐藏不可见
            Cursor.visible = false;

            _gameInputManager.PlayerControl.Enable();
            _gameInputManager.PlayerControl.Move.performed += OnMovementPerformed;
            _gameInputManager.PlayerControl.Move.canceled += OnMovementCanceled;
            _gameInputManager.PlayerControl.Jump.performed += OnJumpPerformed;
            _gameInputManager.PlayerControl.Jump.canceled += OnJumpCanceled;
            _gameInputManager.PlayerControl.ZoomView.performed += OnZoomViewPerformed;
        }

        /// <summary>
        /// 当移动按键按下或持续时的回调
        /// </summary>
        /// <param name="obj">输入上下文</param>
        private void OnMovementPerformed(InputAction.CallbackContext obj)
        {
            var input = obj.ReadValue<Vector2>();
            _eventManager.Publish(GameEvents.PLAYER_MOVE_PERFORMED, input);
        }

        /// <summary>
        /// 当移动按键松开时的回调
        /// </summary>
        /// <param name="obj">输入上下文</param>
        private void OnMovementCanceled(InputAction.CallbackContext obj)
        {
            _eventManager.Publish(GameEvents.PLAYER_MOVE_CANCELED, Vector2.zero);
        }

        /// <summary>
        /// 当跳跃按键按下时的回调
        /// </summary>
        /// <param name="obj">输入上下文</param>
        private void OnJumpPerformed(InputAction.CallbackContext obj)
        {
            _eventManager.Publish<bool>(GameEvents.PLAYER_JUMP_PERFROMED, true);
        }

        /// <summary>
        /// 当跳跃按键松开时的回调
        /// </summary>
        /// <param name="obj">输入上下文</param>
        private void OnJumpCanceled(InputAction.CallbackContext obj)
        {
            _eventManager.Publish<bool>(GameEvents.PLAYER_JUMP_PERFROMED, false);
        }

        /// <summary>
        /// 当缩放视图（鼠标滚轮）操作时的回调
        /// </summary>
        /// <param name="obj">输入上下文</param>
        private void OnZoomViewPerformed(InputAction.CallbackContext obj)
        {
            float rawValue = obj.ReadValue<float>();

            float zoomAmount = Mathf.Clamp(rawValue, -1f, 1f);

            _eventManager.Publish(GameEvents.PLAYER_CAMERA_ZOOM, zoomAmount);
        }

        /// <summary>
        /// 释放资源，取消事件订阅
        /// </summary>
        public override void Dispose()
        {
            _gameInputManager.PlayerControl.Disable();
            _gameInputManager.PlayerControl.Move.performed -= OnMovementPerformed;
            _gameInputManager.PlayerControl.Move.canceled -= OnMovementCanceled;

            _gameInputManager.PlayerControl.Jump.performed -= OnJumpPerformed;
            _gameInputManager.PlayerControl.Jump.canceled -= OnJumpCanceled;

            _gameInputManager.PlayerControl.ZoomView.performed -= OnZoomViewPerformed;
        }
    }
}
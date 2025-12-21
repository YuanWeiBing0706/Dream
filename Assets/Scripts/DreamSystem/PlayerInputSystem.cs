using DreamManager;
using Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DreamSystem
{
    public class PlayerInputSystem : GameSystem
    {
        private readonly GameInputManager _gameInputManager;
        private readonly EventManager _eventManager;

        public PlayerInputSystem(GameInputManager inputManager, EventManager eventManager)
        {
            _gameInputManager = inputManager;
            _eventManager = eventManager;
        }

        public override void Start()
        {
            Cursor.lockState = CursorLockMode.Locked; // 锁定在屏幕中心
            Cursor.visible = false; // 隐藏不可见

            _gameInputManager.PlayerControl.Enable();
            _gameInputManager.PlayerControl.Move.performed += OnMovementPerformed;
            _gameInputManager.PlayerControl.Move.canceled += OnMovementCanceled;
            _gameInputManager.PlayerControl.Jump.performed += OnJumpPerformed;
            _gameInputManager.PlayerControl.Jump.canceled += OnJumpCanceled;
            _gameInputManager.PlayerControl.ZoomView.performed += OnZoomViewPerformed;
        }


        // --- 核心：处理移动输入 ---
        private void OnMovementPerformed(InputAction.CallbackContext obj)
        {
            var input = obj.ReadValue<Vector2>();
            _eventManager.Publish(GameEvents.PLAYER_MOVE_PERFORMED, input);
        }

        private void OnMovementCanceled(InputAction.CallbackContext obj)
        {
            _eventManager.Publish(GameEvents.PLAYER_MOVE_CANCELED, Vector2.zero);
        }
        
        private void OnJumpPerformed(InputAction.CallbackContext obj)
        {
            _eventManager.Publish<bool>(GameEvents.PLAYER_JUMP_PERFROMED, true);
        }

        private void OnJumpCanceled(InputAction.CallbackContext obj)
        {
            _eventManager.Publish<bool>(GameEvents.PLAYER_JUMP_PERFROMED, false);
        }

        
        private void OnZoomViewPerformed(InputAction.CallbackContext obj)
        {
            float rawValue = obj.ReadValue<float>();

            float zoomAmount = Mathf.Clamp(rawValue, -1f, 1f);

            _eventManager.Publish(GameEvents.PLAYER_CAMERA_ZOOM, zoomAmount);
        }

        public override void Dispose()
        {
            _gameInputManager.PlayerControl.Disable();
            _gameInputManager.PlayerControl.Move.performed -= OnMovementPerformed;
            _gameInputManager.PlayerControl.Move.canceled -= OnMovementCanceled;

            _gameInputManager.PlayerControl.Jump.performed += OnJumpPerformed;
            _gameInputManager.PlayerControl.Jump.canceled += OnJumpCanceled;

            _gameInputManager.PlayerControl.ZoomView.performed -= OnZoomViewPerformed;
        }
    }
}
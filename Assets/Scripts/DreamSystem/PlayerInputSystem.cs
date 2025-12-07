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

            _gameInputManager.PlayerControl.Move.performed += OnMovementPerformed;
            _gameInputManager.PlayerControl.Move.canceled += OnMovementCanceled;

            _gameInputManager.Enable();
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


        public override void Dispose()
        {
            // 记得这里也要改名字
            _gameInputManager.PlayerControl.Move.performed -= OnMovementPerformed;
            _gameInputManager.PlayerControl.Move.canceled -= OnMovementCanceled;
        }
    }
}
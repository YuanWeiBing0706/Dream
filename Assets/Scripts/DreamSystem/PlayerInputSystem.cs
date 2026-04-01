using Const;
using DreamManager;
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
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _gameInputManager.PlayerControl.Enable();
            _gameInputManager.PlayerControl.Move.performed += OnMovementPerformed;
            _gameInputManager.PlayerControl.Move.canceled += OnMovementCanceled;
            _gameInputManager.PlayerControl.Jump.performed += OnJumpPerformed;
            _gameInputManager.PlayerControl.Jump.canceled += OnJumpCanceled;
            _gameInputManager.PlayerControl.ZoomView.performed += OnZoomViewPerformed;
            _gameInputManager.PlayerControl.Dodge.performed += OnDodgePerformed;
            _gameInputManager.PlayerControl.Dodge.canceled += OnDodgeCanceled;
            _gameInputManager.PlayerControl.LightAttack.performed += OnLightAttackPerformed;
            _gameInputManager.PlayerControl.LightAttack.canceled += OnLightAttackCanceled;
            _gameInputManager.PlayerControl.HeavyAttack.performed += OnHeavyAttackPerformed;
            _gameInputManager.PlayerControl.HeavyAttack.canceled += OnHeavyAttackCanceled;

        }

        private void OnHeavyAttackCanceled(InputAction.CallbackContext obj)
        {
            _eventManager.Publish(GameEvents.PLAYER_HEAVYATTACK_CANCELED, false);
        }
        private void OnHeavyAttackPerformed(InputAction.CallbackContext obj)
        {
            _eventManager.Publish(GameEvents.PLAYER_HEAVYATTACK_PERFROMED, true);
        }



        private void OnLightAttackPerformed(InputAction.CallbackContext obj)
        {
            _eventManager.Publish(GameEvents.PLAYER_LIGHTATTACK_PERFROMED, true);
        }

        private void OnLightAttackCanceled(InputAction.CallbackContext obj)
        {
            _eventManager.Publish(GameEvents.PLAYER_LIGHTATTACK_CANCELED, false);
        }

        private void OnDodgePerformed(InputAction.CallbackContext obj)
        {
            _eventManager.Publish(GameEvents.PLAYER_DODGE_PERFORMED, true);
        }
        private void OnDodgeCanceled(InputAction.CallbackContext obj)
        {
            _eventManager.Publish(GameEvents.PLAYER_DODGE_CANCELED, false);
        }

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
            _eventManager.Publish(GameEvents.PLAYER_JUMP_PERFROMED, true);
        }

        private void OnJumpCanceled(InputAction.CallbackContext obj)
        {
            _eventManager.Publish(GameEvents.PLAYER_JUMP_CANCELED, false);
        }

        private void OnZoomViewPerformed(InputAction.CallbackContext obj)
        {
            float rawValue = obj.ReadValue<float>();
            float zoomAmount = Mathf.Clamp(rawValue, -1f, 1f);
            _eventManager.Publish(GameEvents.PLAYER_CAMERA_ZOOM, zoomAmount);
        }

        public override void Dispose()
        {
            _gameInputManager.PlayerControl.Enable();
            _gameInputManager.PlayerControl.Move.performed -= OnMovementPerformed;
            _gameInputManager.PlayerControl.Move.canceled -= OnMovementCanceled;
            _gameInputManager.PlayerControl.Jump.performed -= OnJumpPerformed;
            _gameInputManager.PlayerControl.Jump.canceled -= OnJumpCanceled;
            _gameInputManager.PlayerControl.ZoomView.performed -= OnZoomViewPerformed;
            _gameInputManager.PlayerControl.Dodge.performed -= OnDodgePerformed;
            _gameInputManager.PlayerControl.Dodge.canceled -= OnDodgeCanceled;
            _gameInputManager.PlayerControl.LightAttack.performed -= OnLightAttackPerformed;
            _gameInputManager.PlayerControl.LightAttack.canceled -= OnLightAttackCanceled;
        }
    }
}
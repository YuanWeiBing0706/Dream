using Dream;
using Events;
using UnityEngine;
using UnityEngine.InputSystem;
namespace DreamSystem
{
    public class PlayerInputSystem : GameSystem
    {
        ///游戏输入管理器实例
        private readonly GameInputManager _gameInputManager;

        private readonly EventManager _eventManager;

        ///当前移动输入值
        private Vector2 _currentMovement = Vector2.zero;

        ///当前视角移动增量
        private Vector2 _currentLookDelta = Vector2.zero;
        
        private bool _isActive;

        public PlayerInputSystem(GameInputManager inputManager, EventManager eventManager)
        {
            _gameInputManager = inputManager;
            _eventManager = eventManager;
        }

        public override void Start()
        {
            _gameInputManager.PlayerInput_Key.Move.performed += OnMovementPerformed;
            _gameInputManager.PlayerInput_Key.Move.canceled += OnMovementCanceled;

            //鼠标视角的
            _gameInputManager.PlayerInput_Key.AngleView.performed += OnAngleViewPerformed;
            _gameInputManager.PlayerInput_Key.AngleView.canceled += OnAngleViewCanceled;
            _gameInputManager.Enable();
        }
        
        public void Activate()
        {
            _isActive = true;
        }
        
        public override void LateTick()
        {
            if(!_isActive) return;
        }

        private void OnAngleViewPerformed(InputAction.CallbackContext obj)
        {
            _currentMovement = obj.ReadValue<Vector2>();
            _eventManager.Publish(GameEvents.PLAYER_ANGLE_VIEW_PERFORMED, _currentMovement);
        }

        private void OnAngleViewCanceled(InputAction.CallbackContext obj)
        {
            _currentMovement = Vector2.zero;
            _eventManager.Publish(GameEvents.PLAYER_ANGLE_VIEW_CANCELED, _currentMovement);
        }


        private void OnMovementPerformed(InputAction.CallbackContext obj)
        {

        }

        private void OnMovementCanceled(InputAction.CallbackContext obj)
        {

        }


        /// <summary>
        /// 释放输入管理器资源
        /// </summary>
        public override void Dispose()
        {
            _gameInputManager.PlayerInput_Key.Move.performed -= OnMovementPerformed;
            _gameInputManager.PlayerInput_Key.Move.canceled -= OnMovementCanceled;

            _gameInputManager.PlayerInput_Key.AngleView.performed -= OnAngleViewPerformed;
            _gameInputManager.PlayerInput_Key.AngleView.canceled -= OnAngleViewCanceled;
        }
    }
}
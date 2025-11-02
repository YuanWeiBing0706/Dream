using System;
using Events;
using Interface.IUntiy;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dream
{
    [GameSystem(CollectType.Auto)]
    public class PlayerInputSystem : GameSystem, IDisposable
    {
        ///游戏输入管理器实例
        private GameInputManager _gameInputManager;

        ///当前移动输入值
        private Vector2 _currentMovement = Vector2.zero;

        ///当前视角移动增量
        private Vector2 _currentLookDelta = Vector2.zero;

        /// <summary>
        /// 初始化玩家输入系统
        /// </summary>
        public override void Init()
        {
            _gameInputManager = new GameInputManager();

            _gameInputManager.PlayerInput_Key.Move.performed += OnMovementPerformed;
            _gameInputManager.PlayerInput_Key.Move.canceled += OnMovementCanceled;
            //鼠标视角的
            _gameInputManager.PlayerInput_Key.AngleView.performed += OnAngleViewPerformed;
            _gameInputManager.PlayerInput_Key.AngleView.canceled += OnAngleViewCanceled;
            _gameInputManager.Enable();
        }

        private void OnAngleViewPerformed(InputAction.CallbackContext obj)
        {
            _currentMovement = obj.ReadValue<Vector2>();
            EventManager.Instance.Publish(GameEvents.PLAYER_ANGLE_VIEW_PERFORMED, _currentMovement);
        }

        private void OnAngleViewCanceled(InputAction.CallbackContext obj)
        {
            _currentMovement = Vector2.zero;
            EventManager.Instance.Publish(GameEvents.PLAYER_ANGLE_VIEW_CANCELED, _currentMovement);
        }



        /// <summary>
        /// 处理移动输入执行事件
        /// </summary>
        /// <param name="obj">输入动作回调上下文</param>
        private void OnMovementPerformed(InputAction.CallbackContext obj)
        {

        }

        /// <summary>
        /// 处理移动输入取消事件
        /// </summary>
        /// <param name="obj">输入动作回调上下文</param>
        private void OnMovementCanceled(InputAction.CallbackContext obj)
        {

        }

        /// <summary>
        /// 手动释放系统资源
        /// </summary>
        public override void ManualDispose()
        {
            Dispose();
        }

        /// <summary>
        /// 释放输入管理器资源
        /// </summary>
        public void Dispose()
        {
            _gameInputManager?.Dispose();
        }
    }
}
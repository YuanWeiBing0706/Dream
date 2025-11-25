using Cysharp.Threading.Tasks;
using DreamManager;
using Events;
using Model.Player;
using UnityEngine;

namespace DreamSystem.Player
{
    public class PlayerMoveSystem : GameSystem
    {
        /// 移动速度
        public float moveSpeed = 6f;

        /// 转身速度
        public float rotateSpeed = 720f;

        /// 当前的键盘/摇杆输入值 (x=左右, y=前后)
        private Vector2 _moveInput;
        
        /// 玩家数据模型
        private readonly PlayerModel _player;

        /// 事件管理器
        private readonly EventManager _events;

        /// 角色控制器组件（用于处理物理移动）
        private readonly CharacterController _playerCharacterController;

        /// 主相机变换组件（用于计算相对方向）
        private Transform _cameraTransform;

        public PlayerMoveSystem(PlayerModel player, EventManager events, CharacterController playerCharacterController)
        {
            _player = player;
            _events = events;
            _playerCharacterController = playerCharacterController;
        }

        public override void Start()
        {
            if (Camera.main == null)
            {
                return;
            }
            else
            {
                _cameraTransform = Camera.main.transform;
            }

            if (_playerCharacterController == null)
            {
                return;
            }

            _events.Subscribe<Vector2>(GameEvents.PLAYER_MOVE_PERFORMED, OnMovePerformed);
            _events.Subscribe<Vector2>(GameEvents.PLAYER_MOVE_CANCELED, OnMoveCanceled);
        }

        /// 当移动按键按下时的回调
        private void OnMovePerformed(Vector2 input) => _moveInput = input;

        /// 当移动按键松开时的回调
        private void OnMoveCanceled(Vector2 input) => _moveInput = Vector2.zero;

        /// 每帧后期的逻辑更新（处理移动）
        public override void LateTick()
        {
            if (_cameraTransform == null || _playerCharacterController == null) return;

            // 性能优化：如果没有输入（玩家没按键），直接返回，不进行复杂的数学运算
            if (_moveInput.sqrMagnitude < 0.01f) return;

            // 1. 获取相机的方向向量
            // forward 是相机正前方，right 是相机正右方
            Vector3 camForward = _cameraTransform.forward;
            Vector3 camRight = _cameraTransform.right;

            // 2. 压平 Y 轴（重要！）
            // 业务逻辑：相机的 forward 可能指向天空或地面。
            // 如果不把 Y 设为 0，玩家按 W 时就会往天上飞或者钻进地里。
            // 我们只希望在水平面上移动。
            camForward.y = 0;
            camRight.y = 0;
            // 归一化：保证方向向量的长度为 1，防止斜向移动速度变慢/变快
            camForward.Normalize();
            camRight.Normalize();

            // 3. 混合输入方向与相机方向
            // 数学原理：
            // 最终方向 = (相机前方 * 玩家前后输入) + (相机右方 * 玩家左右输入)
            // 举例：相机朝北，玩家按左(A)。targetDirection = 北*0 + 东*-1 = 西。
            Vector3 targetDirection = (camForward * _moveInput.y + camRight * _moveInput.x).normalized;

            // 4. 移动角色 (位移)
            // Move 方法接受：方向 * 速度 * 时间增量
            _playerCharacterController.Move(targetDirection * moveSpeed * Time.deltaTime);

            // [可选] 简易重力：为了防止角色悬空，这里可以补一个向下的力
            // _characterController.Move(Physics.gravity * Time.deltaTime); 

            // 5. 旋转角色 (转向)
            // 业务逻辑：角色不仅要位移，脸还要朝向移动的方向。
            if (targetDirection != Vector3.zero)
            {
                // 计算目标旋转角度：让角色的 Z 轴对准 targetDirection
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                // 平滑旋转 (原神手感)
                // RotateTowards：以恒定的角速度（rotateSpeed）向目标角度转动
                // 相比 Slerp，RotateTowards 更线性、更干脆，没有那种“粘滞”感
                _player.transform.rotation = Quaternion.RotateTowards(
                    _player.transform.rotation,
                    targetRotation,
                    rotateSpeed * Time.deltaTime
                );
            }
        }

        public override void Dispose()
        {
            _events.Unsubscribe<Vector2>(GameEvents.PLAYER_MOVE_PERFORMED, OnMovePerformed).Forget();
            _events.Unsubscribe<Vector2>(GameEvents.PLAYER_MOVE_CANCELED, OnMoveCanceled).Forget();
        }
    }
}
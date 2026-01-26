using DreamManager;
using Events;
using Interface;
using Struct;

namespace DreamSystem.Player
{
    public class PlayerAttackSystem : GameSystem, IPlayerAttackContext
    {
        /// 事件管理器
        private readonly EventManager _eventManager;

        /// 玩家状态机
        private readonly PlayerStateMachine _playerStateMachine;

        /// 攻击输入缓存
        private AttackInputs _attackInputs;

        /// IPlayerAttackContext 实现
        public AttackInputs AttackInputs => _attackInputs;

        public PlayerAttackSystem(EventManager eventManager, PlayerStateMachine playerStateMachine)
        {
            _eventManager = eventManager;
            _playerStateMachine = playerStateMachine;
        }

        /// <summary>
        /// 系统启动：订阅攻击输入事件。
        /// </summary>
        public override void Start()
        {
            _eventManager.Subscribe<bool>(GameEvents.PLAYER_LIGHTATTACK_PERFROMED, OnLightAttackPerformed);
            _eventManager.Subscribe<bool>(GameEvents.PLAYER_LIGHTATTACK_CANCELED, OnLightAttackCanceled);
            _eventManager.Subscribe<bool>(GameEvents.PLAYER_HEAVYATTACK_PERFROMED, OnHeavyAttackPerformed);
        }

        /// <summary>
        /// 轻攻击松开时重置输入标记。
        /// </summary>
        /// <param name="isPressed">是否按下 (canceled 时为 false)</param>
        private void OnLightAttackCanceled(bool isPressed)
        {
            _attackInputs.isLightAttack = isPressed;
        }

        /// <summary>
        /// 轻攻击按下时设置输入标记并触发状态切换。
        /// </summary>
        /// <param name="isPressed">是否按下</param>
        private void OnLightAttackPerformed(bool isPressed)
        {
            _attackInputs.isLightAttack = isPressed;

            if (isPressed)
            {
                _playerStateMachine.TransitionTo(_playerStateMachine.LightAttackState);
            }
        }

        /// <summary>
        /// 重攻击按下时设置输入标记。
        /// </summary>
        /// <param name="isPressed">是否按下</param>
        private void OnHeavyAttackPerformed(bool isPressed)
        {
            _attackInputs.isHeavyAttack = isPressed;

            if (isPressed)
            {
                // TODO: 实现重攻击状态切换
                UnityEngine.Debug.Log("[PlayerAttackSystem] Heavy Attack triggered (Not implemented yet)");
            }
        }

        /// <summary>
        /// 释放资源，取消事件订阅。
        /// </summary>
        public override void Dispose()
        {
            _eventManager.Unsubscribe<bool>(GameEvents.PLAYER_LIGHTATTACK_PERFROMED, OnLightAttackPerformed);
            _eventManager.Unsubscribe<bool>(GameEvents.PLAYER_LIGHTATTACK_CANCELED, OnLightAttackCanceled);
            _eventManager.Unsubscribe<bool>(GameEvents.PLAYER_HEAVYATTACK_PERFROMED, OnHeavyAttackPerformed);
        }
    }
}
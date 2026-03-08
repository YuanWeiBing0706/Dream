using DreamManager;
using Events;
using Interface;
using Struct;

namespace DreamSystem.Player
{
    /// <summary>
    /// 玩家攻击输入系统。
    /// <para>纯逻辑系统（非 MonoBehaviour），由 VContainer 注入生命周期。</para>
    /// <para>职责：接收轻/重攻击输入事件、缓存输入标记、触发状态机切换到对应攻击状态。</para>
    /// <para>同时实现 IPlayerAttackContext，为状态机提供攻击输入数据。</para>
    /// </summary>
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
            _eventManager.Subscribe<bool>(GameEvents.PLAYER_HEAVYATTACK_PERFROMED, OnHeavyAttackPerformed);
        }

        /// <summary>
        /// 轻攻击按下时设置输入标记并触发状态切换。
        /// </summary>
        /// <param name="isPressed">是否按下</param>
        private void OnLightAttackPerformed(bool isPressed)
        {
            if (isPressed)
            {
                _attackInputs.isLightAttack = true;
                _playerStateMachine.TransitionTo(_playerStateMachine.LightAttackState);
            }
        }

        /// <summary>
        /// 重攻击按下时设置输入标记。
        /// </summary>
        /// <param name="isPressed">是否按下</param>
        private void OnHeavyAttackPerformed(bool isPressed)
        {
            if (isPressed && _playerStateMachine.CurrentStateName is "FallState" or "JumpState")
            {
                _attackInputs.isFallAttack = true;
                _playerStateMachine.TransitionTo(_playerStateMachine.FallAttackState);
                return;
            }
            
            if (isPressed)
            {
                _attackInputs.isHeavyAttack = true;
                _playerStateMachine.TransitionTo(_playerStateMachine.HeavyAttackState);
            }
        }
        

        /// <summary>
        /// 消费轻攻击输入：返回当前值并重置为 false。
        /// </summary>
        /// <returns>是否有轻攻击输入</returns>
        public bool ConsumeLightAttack()
        {
            bool result = _attackInputs.isLightAttack;
            _attackInputs.isLightAttack = false;
            return result;
        }

        /// <summary>
        /// 消费重攻击输入：返回当前值并重置为 false。
        /// </summary>
        /// <returns>是否有重攻击输入</returns>
        public bool ConsumeHeavyAttack()
        {
            bool result = _attackInputs.isHeavyAttack;
            _attackInputs.isHeavyAttack = false;
            return result;
        }
        
        /// <summary>
        /// 消费重攻击输入：返回当前值并重置为 false。
        /// </summary>
        /// <returns>是否有下落攻击输入</returns>
        public bool ConsumeFallAttack()
        {
            bool result = _attackInputs.isFallAttack;
            _attackInputs.isFallAttack = false;
            return result;
        }
        

        /// <summary>
        /// 释放资源，取消事件订阅。
        /// </summary>
        public override void Dispose()
        {
            _eventManager.Unsubscribe<bool>(GameEvents.PLAYER_LIGHTATTACK_PERFROMED, OnLightAttackPerformed);
            _eventManager.Unsubscribe<bool>(GameEvents.PLAYER_HEAVYATTACK_PERFROMED, OnHeavyAttackPerformed);
        }
    }
}
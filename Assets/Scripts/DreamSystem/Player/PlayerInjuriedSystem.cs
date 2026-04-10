
using Const;
using DreamManager;
using Enum.Buff;
using Interface;
using Struct;
using DreamSystem.Damage;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DreamSystem.Player
{
    /// <summary>
    /// 玩家受伤系统。
    /// <para>职责：订阅 DAMAGE_RESULT 事件，处理玩家被击中后的反馈（受击动画、UI 更新等）。</para>
    /// </summary>
    public class PlayerInjuriedSystem : GameSystem
    {
        private readonly EventManager _eventManager;
        private readonly IBuffOwner _player;
        private readonly PlayerStateMachine _playerStateMachine;
        private readonly DamageSystem _damageSystem;

        public PlayerInjuriedSystem(EventManager eventManager, IBuffOwner player, PlayerStateMachine playerStateMachine, DamageSystem damageSystem)
        {
            _eventManager = eventManager;
            _player = player;
            _playerStateMachine = playerStateMachine;
            _damageSystem = damageSystem;
        }

        public override void Start()
        {
            _eventManager.Subscribe<DamageResult>(GameEvents.DAMAGE_RESULT, OnDamageResult);

            RegisterPlayerAsync().Forget();
        }

        private async UniTaskVoid RegisterPlayerAsync()
        {
            // 等待 PlayerModel 异步加载完属性
            await UniTask.WaitUntil(() => _player.Stats != null);

            // 拿到玩家身上的 Collider 并注册到 DamageSystem
            var playerMono = _player as MonoBehaviour;
            if (playerMono != null)
            {
                var col = playerMono.GetComponentInChildren<Collider>();
                if (col != null)
                {
                    _damageSystem.Register(col, _player.Stats);
                    UnityEngine.Debug.Log("[PlayerInjuriedSystem] 玩家已注册到伤害系统！");
                }
            }
        }

        /// <summary>
        /// 伤害结算结果回调：判断是否是玩家受伤，然后处理反馈。
        /// </summary>
        private void OnDamageResult(DamageResult result)
        {
            // 判断受击方是否是玩家（比较 CharacterStats 引用）
            if (result.TargetStats != _player.Stats) return;

            UnityEngine.Debug.Log($"[PlayerDamageSystem] 玩家受到 {result.FinalDamage} 点伤害，当前血量: {_player.Stats.GetCurrentStatValue(StatType.Health)}");

            if (result.IsDead)
            {
                UnityEngine.Debug.Log("[PlayerDamageSystem] 玩家死亡！");
                _playerStateMachine.TransitionTo(_playerStateMachine.DeadState);
                _eventManager.Publish(GameEvents.PLAYER_DEAD, true);
                return;
            }
            // 受击动画播放
            _playerStateMachine.TransitionTo(_playerStateMachine.HitState);

            // TODO: 屏幕闪红、更新血条 UI 等
        }

        public override void Dispose()
        {
            _eventManager.Unsubscribe<DamageResult>(GameEvents.DAMAGE_RESULT, OnDamageResult);

            var playerMono = _player as UnityEngine.MonoBehaviour;
            if (playerMono != null)
            {
                var col = playerMono.GetComponentInChildren<UnityEngine.Collider>();
                if (col != null)
                {
                    _damageSystem.Unregister(col);
                }
            }
        }
    }
}
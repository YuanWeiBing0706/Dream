using DreamManager;
using DreamSystem.Damage;
using DreamSystem.Damage.Stat;
using Enum.Buff;
using Events;
using Struct;

namespace DreamSystem.Player
{
    /// <summary>
    /// 玩家受伤系统。
    /// <para>职责：订阅 DAMAGE_RESULT 事件，处理玩家被击中后的反馈（受击动画、UI 更新等）。</para>
    /// </summary>
    public class PlayerInjuriedSystem : GameSystem
    {
        private readonly EventManager _eventManager;
        private readonly CharacterStats _playerStats;

        public PlayerInjuriedSystem(EventManager eventManager, CharacterStats playerStats)
        {
            _eventManager = eventManager;
            _playerStats = playerStats;
        }

        public override void Start()
        {
            _eventManager.Subscribe<DamageResult>(GameEvents.DAMAGE_RESULT, OnDamageResult);
        }

        /// <summary>
        /// 伤害结算结果回调：判断是否是玩家受伤，然后处理反馈。
        /// </summary>
        private void OnDamageResult(DamageResult result)
        {
            // 判断受击方是否是玩家（比较 CharacterStats 引用）
            if (result.TargetStats != _playerStats) return;

            UnityEngine.Debug.Log($"[PlayerDamageSystem] 玩家受到 {result.FinalDamage} 点伤害，当前血量: {_playerStats.GetCurrentStatValue(StatType.Health)}");

            if (result.IsDead)
            {
                UnityEngine.Debug.Log("[PlayerDamageSystem] 玩家死亡！");
                // TODO: 播放死亡动画、显示 Game Over 等
            }

            // TODO: 播放受击动画、屏幕闪红、更新血条 UI 等
        }

        public override void Dispose()
        {
            _eventManager.Unsubscribe<DamageResult>(GameEvents.DAMAGE_RESULT, OnDamageResult);
        }
    }
}

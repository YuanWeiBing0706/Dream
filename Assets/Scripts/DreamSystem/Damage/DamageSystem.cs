using System;
using System.Collections.Generic;
using DreamManager;
using DreamSystem.Damage.Stat;
using Enum.Buff;
using Events;
using Struct;
using UnityEngine;
namespace DreamSystem.Damage
{
    /// <summary>
    /// 伤害管理系统。
    /// <para>职责：维护 Collider → CharacterStats 映射、接收伤害请求、计算伤害公式、扣血、发布结算结果。</para>
    /// </summary>
    public class DamageSystem : GameSystem
    {
        private readonly EventManager _eventManager;
        
        private readonly Dictionary<Collider, CharacterStats> _characterStatsDir = new();

        public DamageSystem(EventManager eventManager)
        {
            _eventManager = eventManager;
        }

        public override void Start()
        {
            _eventManager.Subscribe<DamageRequest>(GameEvents.DAMAGE_REQUEST, OnDamageRequest);
        }

        /// <summary>
        /// 注册可受击单位（Collider → CharacterStats）。
        /// </summary>
        public void Register(Collider collider, CharacterStats characterStats)
        {
            _characterStatsDir[collider] = characterStats;
        }

        /// <summary>
        /// 注销可受击单位。
        /// </summary>
        public void Unregister(Collider collider)
        {
            _characterStatsDir.Remove(collider);
        }

        /// <summary>
        /// 根据 Collider 查找对应的 CharacterStats。
        /// </summary>
        public bool TryGet(Collider collider, out CharacterStats characterStats) => _characterStatsDir.TryGetValue(collider, out characterStats);

        /// <summary>
        /// 伤害请求处理：查表 → 计算公式 → 扣血 → 发布结果。
        /// </summary>
        private void OnDamageRequest(DamageRequest request)
        {
            if (!TryGet(request.TargetCollider, out CharacterStats targetStats))
            {
                return;
            }

            // 伤害公式：基础伤害 + 攻击力 - 防御力
            float atk = request.AttackerStats.GetStat(StatType.Attack).FinalValue;
            float def = targetStats.GetStat(StatType.Defense).FinalValue;
            float finalDamage = Math.Max(request.BaseDamage + atk - def, 0f);

            // 扣血（护盾优先消耗）
            targetStats.ApplyDamage(finalDamage);

            // 判定死亡
            bool isDead = targetStats.GetCurrentStatValue(StatType.Health) <= 0f;

            // 发布结算结果
            _eventManager.Publish(GameEvents.DAMAGE_RESULT, new DamageResult(targetStats, finalDamage, isDead));

            UnityEngine.Debug.Log($"[DamageManager] 伤害结算: 攻={atk}, 防={def}, 最终伤害={finalDamage}, 死亡={isDead}");
        }

        public override void Dispose()
        {
            _eventManager.Unsubscribe<DamageRequest>(GameEvents.DAMAGE_REQUEST, OnDamageRequest);
            _characterStatsDir.Clear();
        }
    }
}
using System.Collections.Generic;
using Const;
using Cysharp.Threading.Tasks;
using DreamManager;
using DreamSystem.Damage;
using DreamSystem.Damage.Stat;
using Enum.Buff;
using Providers;
using Struct;
using UnityEngine;

namespace DreamSystem.Enemy
{
    /// <summary>
    /// 敌人受伤系统。
    /// <para>职责：订阅 ENEMY_SPAWNED 事件，将新生成的敌人注册到伤害系统；
    /// 订阅 DAMAGE_RESULT 事件，处理受伤反馈与死亡广播。</para>
    /// </summary>
    public class EnemyInjuriedSystem : GameSystem
    {
        private readonly EventManager _eventManager;
        private readonly DamageSystem _damageSystem;
        private readonly CharacterStatsFactory _statsFactory;

        /// GameObject → 所有已注册 Collider（用于注销时批量清理）
        private readonly Dictionary<GameObject, Collider[]> _enemyColliders = new();
        /// GameObject → CharacterStats
        private readonly Dictionary<GameObject, CharacterStats> _enemyStats = new();
        /// CharacterStats → GameObject（O(1) 反向查找）
        private readonly Dictionary<CharacterStats, GameObject> _statsToGo = new();
        /// CharacterStats → EnemyStatusSystem
        private readonly Dictionary<CharacterStats, EnemyStatusSystem> _enemyStatusSystems = new();
        /// CharacterStats → EnemyHealthBar
        private readonly Dictionary<CharacterStats, EnemyHealthBar> _enemyHealthBars = new();

        public EnemyInjuriedSystem(EventManager eventManager, DamageSystem damageSystem, CharacterStatsFactory statsFactory)
        {
            _eventManager = eventManager;
            _damageSystem = damageSystem;
            _statsFactory = statsFactory;
        }

        public override void Start()
        {
            _eventManager.Subscribe<EnemySpawnedData>(GameEvents.ENEMY_SPAWNED, OnEnemySpawned);
            _eventManager.Subscribe<DamageResult>(GameEvents.DAMAGE_RESULT, OnDamageResult);
        }

        private void OnEnemySpawned(EnemySpawnedData data)
        {
            if (data.EnemyGo == null)
            {
                UnityEngine.Debug.LogWarning($"[EnemyInjuriedSystem] 收到 ENEMY_SPAWNED 但 EnemyGo 为 null（characterId={data.CharacterId}），跳过注册");
                return;
            }
            RegisterEnemy(data.EnemyGo, data.CharacterId);
        }

        /// <summary>
        /// 将敌人注册到伤害系统，并通过工厂按 characterId 初始化属性。
        /// </summary>
        public void RegisterEnemy(GameObject enemyGo, string characterId = "OrcPADefault")
        {
            if (enemyGo == null)
            {
                UnityEngine.Debug.LogWarning("[EnemyInjuriedSystem] RegisterEnemy 传入 null GO，跳过");
                return;
            }
            if (_enemyStats.ContainsKey(enemyGo))
            {
                UnityEngine.Debug.LogWarning($"[EnemyInjuriedSystem] 敌人 {enemyGo.name} 已注册，跳过重复注册");
                return;
            }

            // 注册所有子 Collider（含根节点），保证无论玩家打到哪个碰撞体都能查到 Stats
            var colliders = enemyGo.GetComponentsInChildren<Collider>(true);
            if (colliders == null || colliders.Length == 0)
            {
                UnityEngine.Debug.LogWarning($"[EnemyInjuriedSystem] 敌人 {enemyGo.name} 缺少 Collider，无法注册");
                return;
            }

            // 通过工厂按 characterId 创建属性，找不到时回退到 "OrcPADefault"
            var stats = _statsFactory.Create(characterId);
            if (stats == null)
            {
                UnityEngine.Debug.LogWarning($"[EnemyInjuriedSystem] 找不到 {characterId} 的属性配置，使用 OrcPADefault 兜底");
                stats = _statsFactory.Create("OrcPADefault");
            }

            foreach (var col in colliders)
                _damageSystem.Register(col, stats);

            var combatSystem = enemyGo.GetComponentInChildren<EnemyCombatSystem>();
            if (combatSystem != null)
            {
                combatSystem.Initialize(stats, _eventManager);
            }

            var statusSystem = enemyGo.GetComponentInChildren<EnemyStatusSystem>();
            if (statusSystem != null)
                _enemyStatusSystems[stats] = statusSystem;

            var healthBar = enemyGo.GetComponentInChildren<EnemyHealthBar>();
            if (healthBar != null)
            {
                float maxHp = stats.GetStat(StatType.Health).FinalValue;
                healthBar.UpdateHp(maxHp, maxHp);
                _enemyHealthBars[stats] = healthBar;
            }

            _enemyColliders[enemyGo] = colliders;
            _enemyStats[enemyGo] = stats;
            _statsToGo[stats] = enemyGo;

            UnityEngine.Debug.Log($"[EnemyInjuriedSystem] 敌人 {enemyGo.name} ({characterId}) 已注册到伤害系统");
        }

        /// <summary>
        /// 从伤害系统注销敌人（怪物归还对象池前调用）。
        /// </summary>
        public void UnregisterEnemy(GameObject enemyGo)
        {
            if (_enemyColliders.TryGetValue(enemyGo, out var colliders))
            {
                foreach (var col in colliders)
                    _damageSystem.Unregister(col);
                _enemyColliders.Remove(enemyGo);
            }

            if (_enemyStats.TryGetValue(enemyGo, out var stats))
            {
                _statsToGo.Remove(stats);
                _enemyStatusSystems.Remove(stats);
                _enemyHealthBars.Remove(stats);
                _enemyStats.Remove(enemyGo);
            }

            UnityEngine.Debug.Log($"[EnemyInjuriedSystem] 敌人 {enemyGo.name} 已从伤害系统注销");
        }

        private void OnDamageResult(DamageResult result)
        {
            if (!_statsToGo.TryGetValue(result.TargetStats, out var enemyGo)) return;

            // 已死亡的敌人不再处理（防止尸体被反复命中产生重复事件）
            if (_enemyStatusSystems.TryGetValue(result.TargetStats, out var statusSystem) && statusSystem.IsDead)
                return;

            float currentHp = result.TargetStats.GetCurrentStatValue(StatType.Health);
            float maxHp = result.TargetStats.GetStat(StatType.Health).FinalValue;

            UnityEngine.Debug.Log($"[EnemyInjuriedSystem] 敌人 {enemyGo.name} 受到 {result.FinalDamage} 点伤害，" +
                                  $"剩余血量: {currentHp} / {maxHp}");

            if (_enemyHealthBars.TryGetValue(result.TargetStats, out var healthBar))
                healthBar.UpdateHp(currentHp, maxHp);

            if (statusSystem == null) return;

            if (result.IsDead)
            {
                UnityEngine.Debug.Log($"[EnemyInjuriedSystem] 敌人 {enemyGo.name} 死亡！");
                // 先标记死亡、发布事件，再注销（避免事件处理时找不到数据）
                statusSystem.SetDead(true);
                _eventManager.Publish(GameEvents.ENEMY_DEATH, enemyGo);
                UnregisterEnemy(enemyGo);
                return;
            }

            statusSystem.SetHit(true);
        }

        public override void Dispose()
        {
            _eventManager.Unsubscribe<EnemySpawnedData>(GameEvents.ENEMY_SPAWNED, OnEnemySpawned).Forget();
            _eventManager.Unsubscribe<DamageResult>(GameEvents.DAMAGE_RESULT, OnDamageResult).Forget();

            foreach (var kvp in _enemyColliders)
            {
                if (kvp.Value == null) continue;
                foreach (var col in kvp.Value)
                    if (col != null) _damageSystem.Unregister(col);
            }

            _enemyColliders.Clear();
            _enemyStats.Clear();
            _statsToGo.Clear();
            _enemyStatusSystems.Clear();
            _enemyHealthBars.Clear();
        }
    }
}

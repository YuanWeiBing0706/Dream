using System.Collections.Generic;
using Animancer;
using DreamManager;
using DreamSystem.Damage;
using DreamSystem.Damage.Stat;
using Enum.Buff;
using Events;
using SO;
using Struct;
using UnityEngine;

namespace DreamSystem.Enemy
{
    /// <summary>
    /// 敌人受伤系统。
    /// <para>职责：注册/注销敌人到伤害系统、订阅伤害结算结果、处理敌人受伤反馈。</para>
    /// </summary>
    public class EnemyInjuriedSystem : GameSystem
    {
        private readonly EventManager _eventManager;
        private readonly DamageSystem _damageSystem;

        /// 存储每个敌人 GameObject 对应的 CharacterStats
        private readonly Dictionary<GameObject, CharacterStats> _enemyStats = new();

        /// 存储每个敌人 GameObject 对应的 Collider（避免重复查找）
        private readonly Dictionary<GameObject, Collider> _enemyColliders = new();

        /// 存储每个敌人 CharacterStats 对应的 EnemyStatusSystem（用于受击/死亡状态设值）
        private readonly Dictionary<CharacterStats, EnemyStatusSystem> _enemyStatusSystems = new();

        public EnemyInjuriedSystem(EventManager eventManager, DamageSystem damageSystem)
        {
            _eventManager = eventManager;
            _damageSystem = damageSystem;
        }

        public override void Start()
        {
            _eventManager.Subscribe<DamageResult>(GameEvents.DAMAGE_RESULT, OnDamageResult);

            // [测试用] 扫描场景中已存在的敌人并注册
            ScanSceneEnemies();
        }

        private void ScanSceneEnemies()
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            UnityEngine.Debug.Log($"[EnemyDamageSystem] 扫描到 {enemies.Length} 个场景敌人");

            foreach (var enemyGo in enemies)
            {
                RegisterEnemy(enemyGo);
            }
        }

        /// <summary>
        /// 注册敌人到伤害系统。
        /// <para>创建 CharacterStats、注册到 DamageManager、初始化 EnemyCombatSystem。</para>
        /// </summary>
        public void RegisterEnemy(GameObject enemyGo)
        {
            // 从预制体上的 [SerializeField] 引用获取 Collider
            var enemyModel = enemyGo.GetComponentInChildren<Collider>();
            if (enemyModel == null)
            {
                UnityEngine.Debug.LogWarning($"[EnemyDamageSystem] 敌人 {enemyGo.name} 没有 Collider，无法注册");
                return;
            }

            var collider = enemyModel;

            // 创建属性
            CharacterStats characterStats = new CharacterStats();

            // 注册到 DamageManager 查找表
            _damageSystem.Register(collider, characterStats);

            // 初始化 EnemyCombatSystem（传递 CharacterStats 和 EventManager）
            var combatSystem = enemyGo.GetComponentInChildren<EnemyCombatSystem>();
            if (combatSystem != null)
            {
                combatSystem.Initialize(characterStats, _eventManager);
            }

            // 获取并缓存该敌人自己的 EnemyStatusSystem
            var statusSystem = enemyGo.GetComponentInChildren<EnemyStatusSystem>();
            if (statusSystem != null)
            {
                _enemyStatusSystems[characterStats] = statusSystem;
            }

            // 存储引用
            _enemyStats[enemyGo] = characterStats;
            _enemyColliders[enemyGo] = collider;

            UnityEngine.Debug.Log($"[EnemyDamageSystem] 敌人 {enemyGo.name} 已注册到伤害系统");
        }

        /// <summary>
        /// 从伤害系统注销敌人。
        /// </summary>
        public void UnregisterEnemy(GameObject enemyGo)
        {
            if (_enemyColliders.TryGetValue(enemyGo, out var collider))
            {
                _damageSystem.Unregister(collider);
                _enemyColliders.Remove(enemyGo);
            }

            if (_enemyStats.TryGetValue(enemyGo, out var stats))
            {
                _enemyStatusSystems.Remove(stats);
                _enemyStats.Remove(enemyGo);
            }

            UnityEngine.Debug.Log($"[EnemyDamageSystem] 敌人 {enemyGo.name} 已从伤害系统注销");
        }

        /// <summary>
        /// 伤害结算结果回调：判断是否是敌人受伤，处理反馈。
        /// </summary>
        private void OnDamageResult(DamageResult result)
        {
            // 检查受击方是否是我们管理的敌人
            if (!_enemyStats.ContainsValue(result.TargetStats)) return;

            UnityEngine.Debug.Log($"[EnemyDamageSystem] 敌人受到 {result.FinalDamage} 点伤害，当前血量: {result.TargetStats.GetCurrentStatValue(StatType.Health)}");

            // 通过 TargetStats 找到该敌人的 StatusSystem，设置行为树条件标志
            if (!_enemyStatusSystems.TryGetValue(result.TargetStats, out var statusSystem)) return;

            if (result.IsDead)
            {
                UnityEngine.Debug.Log("[EnemyDamageSystem] 敌人死亡！");
                statusSystem.SetDead(true);
                return;
            }

            statusSystem.SetHit(true);
        }

        public override void Dispose()
        {
            _eventManager.Unsubscribe<DamageResult>(GameEvents.DAMAGE_RESULT, OnDamageResult);

            foreach (var kvp in _enemyColliders)
            {
                if (kvp.Value != null)
                {
                    _damageSystem.Unregister(kvp.Value);
                }
            }

            _enemyStats.Clear();
            _enemyColliders.Clear();
            _enemyStatusSystems.Clear();
        }
    }
}
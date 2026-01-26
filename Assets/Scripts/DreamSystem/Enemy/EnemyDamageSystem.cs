using System.Collections.Generic;
using DreamManager;
using Function.Damageable;
using UnityEngine;

namespace DreamSystem.Enemy
{
    public class EnemyDamageSystem : GameSystem
    {
        private readonly EventManager _eventManager;
        private readonly DamageManager _damageManager;

        // 存储每个敌人 GameObject 对应的 Handler，用于回收时注销
        private readonly Dictionary<GameObject, EnemyDamageHandler> _enemyHandlers = new Dictionary<GameObject, EnemyDamageHandler>();

        public EnemyDamageSystem(EventManager eventManager, DamageManager damageManager)
        {
            _eventManager = eventManager;
            _damageManager = damageManager;
        }

        public override void Start()
        {
            // 订阅敌人受伤/死亡事件 (通过 EventManager)
            _eventManager.Subscribe<EnemyDamageData>(Events.GameEvents.ENEMY_DAMAGED, OnEnemyDamaged);
            _eventManager.Subscribe<EnemyDamageHandler>(Events.GameEvents.ENEMY_DEATH, OnEnemyDeath);

            // [测试用] 扫描场景中已存在的敌人并注册
            ScanSceneEnemies();
        }

        /// <summary>
        /// [测试用] 扫描场景中已存在的敌人并注册到伤害系统。
        /// </summary>
        private void ScanSceneEnemies()
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");

            UnityEngine.Debug.Log($"[EnemyDamageSystem] 扫描到 {enemies.Length} 个场景敌人");

            foreach (var enemyGO in enemies)
            {
                RegisterEnemy(enemyGO);
            }
        }

        /// <summary>
        /// 注册敌人到伤害系统（可由外部调用，如对象池生成后）。
        /// </summary>
        public void RegisterEnemy(GameObject enemyGO)
        {
            var collider = enemyGO.GetComponent<Collider>();
            if (collider == null)
            {
                UnityEngine.Debug.LogWarning($"[EnemyDamageSystem] 敌人 {enemyGO.name} 没有 Collider，无法注册到伤害系统");
                return;
            }

            // 创建该敌人专属的 Handler
            float maxHp = 100f; // TODO: 从敌人配置读取
            var handler = new EnemyDamageHandler(maxHp, _eventManager);

            // 注册到 DamageManager
            _damageManager.Register(collider, handler);

            // 存储引用
            _enemyHandlers[enemyGO] = handler;

            UnityEngine.Debug.Log($"[EnemyDamageSystem] 敌人 {enemyGO.name} 已注册到伤害系统");
        }

        /// <summary>
        /// 从伤害系统注销敌人（可由外部调用，如对象池回收前）。
        /// </summary>
        public void UnregisterEnemy(GameObject enemyGO)
        {
            var collider = enemyGO.GetComponent<Collider>();
            if (collider != null)
            {
                _damageManager.Unregister(collider);
            }

            if (_enemyHandlers.ContainsKey(enemyGO))
            {
                _enemyHandlers.Remove(enemyGO);
            }

            UnityEngine.Debug.Log($"[EnemyDamageSystem] 敌人 {enemyGO.name} 已从伤害系统注销");
        }

        private void OnEnemyDamaged(EnemyDamageData data)
        {
            UnityEngine.Debug.Log($"[EnemyDamageSystem] 敌人受到 {data.Amount} 点伤害，剩余血量: {data.Handler.CurrentHp}");

            // TODO: 在这里添加复杂的受伤逻辑
        }

        private void OnEnemyDeath(EnemyDamageHandler handler)
        {
            UnityEngine.Debug.Log($"[EnemyDamageSystem] 敌人死亡！");

            // TODO: 在这里添加死亡逻辑
        }

        public EnemyDamageHandler GetHandler(GameObject enemyGO)
        {
            return _enemyHandlers.TryGetValue(enemyGO, out var handler) ? handler : null;
        }

        public override void Dispose()
        {
            // 取消订阅 EventManager 事件
            _eventManager.Unsubscribe<EnemyDamageData>(Events.GameEvents.ENEMY_DAMAGED, OnEnemyDamaged);
            _eventManager.Unsubscribe<EnemyDamageHandler>(Events.GameEvents.ENEMY_DEATH, OnEnemyDeath);

            // 清理所有 Handler
            foreach (var kvp in _enemyHandlers)
            {
                var collider = kvp.Key.GetComponent<Collider>();
                if (collider != null)
                {
                    _damageManager.Unregister(collider);
                }
            }
            _enemyHandlers.Clear();
        }
    }
}
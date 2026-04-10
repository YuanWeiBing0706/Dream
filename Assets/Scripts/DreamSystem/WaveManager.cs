using System.Collections.Generic;
using Const;
using Cysharp.Threading.Tasks;
using DreamManager;
using DreamPool;
using Model.Player;
using Struct;
using UnityEngine;
using UnityEngine.AI;
using VContainer;

namespace DreamSystem
{
    public class WaveManager : GameSystem
    {
        private readonly PoolManager _poolManager;
        private readonly EventManager _eventManager;
        private readonly PlayerModel _playerModel;

        private int _totalWaveEnemiesToSpawn = 12;
        private int _currentOnFieldLimit = 6;

        private int _spawnedThisWave = 0;
        private int _deadThisWave = 0;
        private readonly List<GameObject> _activeEnemies = new List<GameObject>();

        private string[] _monsterPool;
        private bool _isWaveActive = false;

        [Inject]
        public WaveManager(PoolManager poolManager, EventManager eventManager, PlayerModel playerModel)
        {
            _poolManager = poolManager;
            _eventManager = eventManager;
            _playerModel = playerModel;
        }
        
        public override void Start()
        {
            _eventManager.Subscribe<string[]>(GameEvents.START_WAVE_REQUEST, OnStartWaveRequested);
            _eventManager.Subscribe<string[]>(GameEvents.START_BOSS_WAVE_REQUEST, OnStartBossWaveRequested);
            _eventManager.Subscribe<GameObject>(GameEvents.ENEMY_DEATH, OnEnemyDead);
        }

        public void StartWave(string[] monPool)
        {
            _monsterPool = monPool;
            _spawnedThisWave = 0;
            _deadThisWave = 0;
            _activeEnemies.Clear();
            _totalWaveEnemiesToSpawn = 12;
            _currentOnFieldLimit = 6;
            _isWaveActive = true;

            UnityEngine.Debug.Log($"[WaveManager] 波次开始！怪池: {string.Join(",", monPool)}");

            CheckAndRefill();
        }

        /// <summary>
        /// 启动 Boss 波次：只生成 1 只 Boss，场上上限也为 1。
        /// </summary>
        public void StartBossWave(string[] bossPool)
        {
            _monsterPool = bossPool;
            _spawnedThisWave = 0;
            _deadThisWave = 0;
            _activeEnemies.Clear();
            _totalWaveEnemiesToSpawn = 1;
            _currentOnFieldLimit = 1;
            _isWaveActive = true;

            string bossName = (bossPool != null && bossPool.Length > 0) ? bossPool[0] : "unknown";
            UnityEngine.Debug.Log($"[WaveManager] Boss 波次开始！Boss: {bossName}");

            CheckAndRefill();
        }

        public override void Tick()
        {
            if (!_isWaveActive) return;
            CheckAndRefill();
        }
        
        private void OnStartWaveRequested(string[] enemyPool) => StartWave(enemyPool);
        private void OnStartBossWaveRequested(string[] bossPool) => StartBossWave(bossPool);

        private void CheckAndRefill()
        {
            if (!_isWaveActive) return;

            while (_activeEnemies.Count < _currentOnFieldLimit && _spawnedThisWave < _totalWaveEnemiesToSpawn)
            {
                SpawnMonster();
            }
        }

        private void SpawnMonster()
        {
            if (_monsterPool == null || _monsterPool.Length == 0) return;

            string monsterId = _monsterPool[Random.Range(0, _monsterPool.Length)];
            Vector3 spawnPos = GetValidSpawnPosition();

            GameObject mon = _poolManager.Release(monsterId, spawnPos, Quaternion.identity);

            // Release 返回 null 表示资产不存在（YooAsset 中未注册），
            // 跳过本次生成并计入已生成数，防止波次永远无法完成
            if (mon == null)
            {
                _spawnedThisWave++;
                _deadThisWave++; // 同步递增死亡数，维持波次完成条件一致
                UnityEngine.Debug.LogWarning($"[WaveManager] 怪物资产不存在，跳过生成: {monsterId}（请检查 YooAsset 是否已注册该预制体）");
                return;
            }

            _activeEnemies.Add(mon);
            _spawnedThisWave++;

            // 通知 EnemyInjuriedSystem 将新生成的怪物注册到伤害系统
            _eventManager.Publish(GameEvents.ENEMY_SPAWNED, new EnemySpawnedData
            {
                EnemyGo = mon,
                CharacterId = monsterId
            });

            UnityEngine.Debug.Log($"[WaveManager] 生成怪物: {monsterId} ({_spawnedThisWave}/{_totalWaveEnemiesToSpawn})");
        }

        /// <summary>
        /// 在玩家周围随机找一个 NavMesh 上的合法出生点，避免怪物生成在墙内。
        /// </summary>
        private Vector3 GetValidSpawnPosition()
        {
            Vector3 playerPos = _playerModel.transform.position;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                float distance = Random.Range(10f, 15f);
                Vector3 candidate = playerPos + new Vector3(randomDir.x, 0f, randomDir.y) * distance;

                // 在候选点附近 3 米范围内查找最近 NavMesh 点
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                    return hit.position;
            }

            // 10 次都找不到则退而求其次：直接取玩家旁边 NavMesh 点
            if (NavMesh.SamplePosition(playerPos + Vector3.forward * 12f, out NavMeshHit fallback, 5f, NavMesh.AllAreas))
                return fallback.position;

            return playerPos + Vector3.forward * 12f;
        }

        public void OnEnemyDead(GameObject enemy)
        {
            if (!_activeEnemies.Contains(enemy)) return;

            _activeEnemies.Remove(enemy);
            _deadThisWave++;

            UnityEngine.Debug.Log($"[WaveManager] 怪物死亡进度: {_deadThisWave}/{_totalWaveEnemiesToSpawn}");

            if (_deadThisWave >= _totalWaveEnemiesToSpawn)
            {
                _isWaveActive = false;
                UnityEngine.Debug.Log("[WaveManager] 波次全部消灭完毕！");
                _eventManager.Publish(GameEvents.WAVE_COMPLETED);
            }
        }
        
        public override void Dispose()
        {
            _eventManager.Unsubscribe<string[]>(GameEvents.START_WAVE_REQUEST, OnStartWaveRequested).Forget();
            _eventManager.Unsubscribe<string[]>(GameEvents.START_BOSS_WAVE_REQUEST, OnStartBossWaveRequested).Forget();
            _eventManager.Unsubscribe<GameObject>(GameEvents.ENEMY_DEATH, OnEnemyDead).Forget();
            _activeEnemies.Clear();
            _isWaveActive = false;
        }
    }
}

using System.Collections.Generic;
using DreamManager;
using DreamSystem.Damage;
using DreamSystem.Damage.Stat;
using Events;
using Model.Player;
using Struct;
using UnityEngine;

namespace DreamSystem.Player
{
    /// <summary>
    /// 玩家战斗系统。
    /// <para>职责：监听攻击检测窗口事件、执行 HitBox 物理检测、发布伤害请求事件。</para>
    /// </summary>
    public class PlayerCombatSystem : GameSystem
    {
        private readonly PlayerHitBox[] _playerHitBoxes;
        private readonly EventManager _eventManager;
        private readonly CharacterStats _characterStats;

        /// 是否正在检测伤害
        private bool _isDetection;

        /// 本次攻击已命中的目标去重集合（按 HitBox 独立计算）
        private readonly HashSet<(int hitBoxIndex, int targetId)> _hitEnemyIds = new();

        public PlayerCombatSystem(PlayerHitBox[] playerHitBoxes, EventManager eventManager, CharacterStats characterStats)
        {
            _playerHitBoxes = playerHitBoxes;
            _eventManager = eventManager;
            _characterStats = characterStats;
        }

        public override void Start()
        {
            _eventManager.Subscribe(GameEvents.PLAYER_ATTACK_OPEN_DETECTION, PlayerAttackOpenDetection);
            _eventManager.Subscribe(GameEvents.PLAYER_ATTACK_CLOSE_DETECTION, PlayerAttackCloseDetection);
        }

        /// <summary>
        /// 每帧后期更新：检测窗口开启时，遍历所有 HitBox 执行物理检测，命中后发布伤害请求。
        /// </summary>
        public override void LateTick()
        {
            if (!_isDetection) return;

            for (int h = 0; h < _playerHitBoxes.Length; h++)
            {
                int hitCount = _playerHitBoxes[h].Detect(out Collider[] colliders);

                for (int i = 0; i < hitCount; i++)
                {
                    Collider target = colliders[i];
                    int targetId = target.GetInstanceID();
                    var hitKey = (h, targetId);

                    if (!_hitEnemyIds.Contains(hitKey))
                    {
                        // 发布伤害请求，由 DamageManager 处理
                        _eventManager.Publish(GameEvents.DAMAGE_REQUEST, new DamageRequest(_characterStats, target, 10f));
                        _hitEnemyIds.Add(hitKey);
                    }
                }
            }
        }

        private void PlayerAttackOpenDetection()
        {
            _isDetection = true;
            _hitEnemyIds.Clear();
        }

        private void PlayerAttackCloseDetection()
        {
            _isDetection = false;
        }

        public override void Dispose()
        {
            _eventManager.Unsubscribe(GameEvents.PLAYER_ATTACK_OPEN_DETECTION, PlayerAttackOpenDetection);
            _eventManager.Unsubscribe(GameEvents.PLAYER_ATTACK_CLOSE_DETECTION, PlayerAttackCloseDetection);
        }
    }
}
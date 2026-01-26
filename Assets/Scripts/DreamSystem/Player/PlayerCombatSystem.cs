using System.Collections.Generic;
using DreamManager;
using Events;
using Function.Damageable;
using Model.Player;
using UnityEngine;

namespace DreamSystem.Player
{
    public class PlayerCombatSystem : GameSystem
    {
        private PlayerHitBox _playerHitBox;
        private EventManager _eventManager;
        private DamageManager _damageManager;
        private bool _isDetection;

        private HashSet<int> _hitEnemyIds = new HashSet<int>();

        public PlayerCombatSystem(PlayerHitBox playerHitBox, EventManager eventManager,DamageManager damageManager)
        {
            _playerHitBox = playerHitBox;
            _eventManager = eventManager;
            _damageManager = damageManager;
        }

        public override void Start()
        {
            _eventManager.Subscribe(GameEvents.PLAYER_ATTACK_OPEN_DETECTION, PlayerAttackOpenDetection);
            _eventManager.Subscribe(GameEvents.PLAYER_ATTACK_CLOSE_DETECTION, PlayerAttackCloseDetection);
        }

        public override void LateTick()
        {
            if (_isDetection)
            {
                // 让 HitBox 进行物理检测
                int hitCount = _playerHitBox.Detect(out Collider[] colliders);
                
                for (int i = 0; i < hitCount; i++)
                {
                    Collider target = colliders[i];
                    int targetId = target.GetInstanceID(); // 刚体的唯一 ID

                    // 检查去重：如果这个怪这次没被打过
                    if (!_hitEnemyIds.Contains(targetId))
                    {
                        UnityEngine.Debug.Log($"🔪 砍到了新敌人: {target.name}");
                        _damageManager.TryGet(target, out IDamageable handler);
                        handler?.TakeDamage(5);//测试用的伤害
                        // 加入受害者名单，防止下一帧重复伤害
                        _hitEnemyIds.Add(targetId);
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
            // 别忘了取消订阅
             _eventManager.Unsubscribe(GameEvents.PLAYER_ATTACK_OPEN_DETECTION, PlayerAttackOpenDetection);
             _eventManager.Unsubscribe(GameEvents.PLAYER_ATTACK_CLOSE_DETECTION, PlayerAttackCloseDetection);
        }
    }
}
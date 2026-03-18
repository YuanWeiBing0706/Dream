
using System.Collections.Generic;
using Animancer;
using DreamManager;
using DreamSystem.Damage;
using DreamSystem.Damage.Stat;
using Events;
using Model.Enemy;
using Sirenix.OdinInspector;
using Struct;
using UnityEngine;

namespace DreamSystem.Enemy
{
    /// <summary>
    /// 敌人战斗系统。
    /// <para>挂在敌人预制体上，供行为树调用。</para>
    /// <para>职责：管理攻击动画、检测窗口、发布伤害请求事件、攻击冷却控制。</para>
    /// </summary>
    public class EnemyCombatSystem : MonoBehaviour
    {
        [SerializeField] private EnemyHitBox[] EnemyHitBoxes;
        [SerializeField] private EnemyAttackData[] AttackDataList;
        [SerializeField] private float AttackCooldown = 2f;
        [SerializeField] private AnimancerComponent Animancer;
        [SerializeField][DrawWithUnity] private ClipTransition IdleClip;

        /// 敌人自身的属性（由 EnemyDamageSystem 注册时赋值）
        private CharacterStats _characterStats;

        /// 事件管理器（由 EnemyDamageSystem 注册时赋值）
        private EventManager _eventManager;

        private bool _isDetecting;
        private bool _isAttacking;
        private float _cooldownTimer;
        private EnemyAttackData _currentAttack;
        private AnimancerState _currentAnimState;
        private readonly HashSet<(int hitBoxIndex, int targetId)> _hitTargetIds = new();

        /// <summary>
        /// 由 EnemyDamageSystem 调用，注入属性和事件管理器。
        /// </summary>
        public void Initialize(CharacterStats characterStats, EventManager eventManager)
        {
            _characterStats = characterStats;
            _eventManager = eventManager;
        }

        private void Start()
        {
            if (IdleClip != null && IdleClip.Clip != null)
            {
                Animancer.Play(IdleClip);
            }
        }

        private void Update()
        {
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
            }

            if (_isAttacking && _currentAnimState != null)
            {
                float progress = _currentAnimState.NormalizedTime;

                if (progress < 1f)
                {
                    UpdateDetectionWindow(progress);
                }
                else
                {
                    CloseDetectionIfOpen();
                    _isAttacking = false;
                    if (IdleClip != null && IdleClip.Clip != null)
                    {
                        Animancer.Play(IdleClip);
                    }
                    _currentAnimState = null;
                }
            }

            if (_isDetecting)
            {
                int[] activeIndices = _currentAttack.activeHitBoxIndices;

                for (int a = 0; a < activeIndices.Length; a++)
                {
                    int h = activeIndices[a];
                    int hitCount = EnemyHitBoxes[h].Detect(out Collider[] colliders);

                    for (int i = 0; i < hitCount; i++)
                    {
                        Collider target = colliders[i];
                        int targetId = target.GetInstanceID();
                        var hitKey = (h, targetId);

                        if (!_hitTargetIds.Contains(hitKey))
                        {
                            // 发布伤害请求，由 DamageManager 处理
                            _eventManager.Publish(GameEvents.DAMAGE_REQUEST, new DamageRequest(_characterStats, target, _currentAttack.damage));
                            _hitTargetIds.Add(hitKey);
                        }
                    }
                }
            }
        }

        private void UpdateDetectionWindow(float progress)
        {
            bool shouldBeOpen = progress >= _currentAttack.hitWindowStart && progress < _currentAttack.hitWindowEnd;

            if (shouldBeOpen && !_isDetecting)
            {
                _isDetecting = true;
                _hitTargetIds.Clear();
            }
            else if (!shouldBeOpen && _isDetecting)
            {
                _isDetecting = false;
            }
        }

        private void CloseDetectionIfOpen()
        {
            if (_isDetecting)
            {
                _isDetecting = false;
            }
        }

        // ===== 以下方法供行为树调用 =====

        public void StartAttack()
        {
            StartAttack(0);
        }

        public void StartAttack(int attackIndex)
        {
            if (attackIndex < 0 || attackIndex >= AttackDataList.Length)
            {
                UnityEngine.Debug.LogWarning($"[EnemyCombat] 无效的攻击索引: {attackIndex}");
                return;
            }

            _currentAttack = AttackDataList[attackIndex];
            _currentAnimState = Animancer.Play(_currentAttack.clip);
            _cooldownTimer = AttackCooldown;
            _isAttacking = true;
        }

        public bool IsAttackReady()
        {
            return _cooldownTimer <= 0f;
        }
    }
}

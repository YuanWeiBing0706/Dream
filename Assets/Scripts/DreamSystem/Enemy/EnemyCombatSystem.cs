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
using UnityEngine.AI;

namespace DreamSystem.Enemy
{
    public class EnemyCombatSystem : MonoBehaviour
    {
        /// 敌人攻击碰撞检测盒数组
        [SerializeField]
        private EnemyHitBox[] EnemyHitBoxes;

        /// 攻击冷却时间（秒）
        [SerializeField]
        private float AttackCooldown = 2f;


        /// 敌人动画系统（负责接管所有的动画播放）
        [SerializeField]
        private EnemyAnimationSystem AnimationSystem;

        /// 敌人自身的属性（由 EnemyDamageSystem 注册时赋值）
        private CharacterStats _characterStats;

        /// 事件管理器（由 EnemyDamageSystem 注册时赋值）
        private EventManager _eventManager;

        /// 当前是否处于检测窗口
        private bool _isDetecting;

        /// 当前是否正在攻击中
        private bool _isAttacking;

        /// 攻击冷却计时器
        private float _cooldownTimer;

        /// 当前正在执行的攻击数据
        private EnemyAttackData _currentAttack;

        /// 当前攻击动画的 Animancer 状态
        private AnimancerState _currentAnimState;

        /// 本次攻击窗口内已命中的目标集合（按 HitBox 索引 + 目标 InstanceID 去重）
        private readonly HashSet<(int hitBoxIndex, int targetId)> _hitTargetIds = new();

        /// <summary>
        /// 由 EnemyDamageSystem 调用，注入敌人属性和事件管理器。
        /// </summary>
        /// <param name="characterStats">敌人的角色属性</param>
        /// <param name="eventManager">全局事件管理器</param>
        public void Initialize(CharacterStats characterStats, EventManager eventManager)
        {
            _characterStats = characterStats;
            _eventManager = eventManager;
        }

        /// <summary>
        /// 初始化时检查依赖
        /// </summary>
        private void Start()
        {
            if (AnimationSystem == null)
            {
                AnimationSystem = GetComponent<EnemyAnimationSystem>();
            }
        }

        /// <summary>
        /// 每帧更新：驱动冷却计时、攻击动画进度和 HitBox 碰撞检测。
        /// </summary>
        private void Update()
        {
            // 冷却计时递减
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
            }

            // 攻击动画进度驱动
            if (_isAttacking && _currentAnimState != null)
            {
                float progress = _currentAnimState.NormalizedTime;

                if (progress < 1f)
                {
                    // 根据动画进度开关检测窗口
                    UpdateDetectionWindow(progress);
                }
                else
                {
                    // 动画播放完毕，关闭检测
                    CloseDetectionIfOpen();
                    _isAttacking = false;
                    _currentAnimState = null;
                }
            }

            // 检测窗口内进行碰撞检测并发布伤害请求
            if (_isDetecting)
            {
                int[] activeIndices = _currentAttack.ActiveHitBoxIndices;

                for (int a = 0; a < activeIndices.Length; a++)
                {
                    int h = activeIndices[a];
                    int hitCount = EnemyHitBoxes[h].Detect(out Collider[] colliders);

                    for (int i = 0; i < hitCount; i++)
                    {
                        Collider target = colliders[i];
                        int targetId = target.GetInstanceID();
                        var hitKey = (h, targetId);

                        // 同一检测窗口内对同一目标只命中一次
                        if (!_hitTargetIds.Contains(hitKey))
                        {
                            _eventManager.Publish(GameEvents.DAMAGE_REQUEST, new DamageRequest(_characterStats, target, _currentAttack.Damage));
                            _hitTargetIds.Add(hitKey);
                        }
                    }
                }
            }


        }

        /// <summary>
        /// 根据动画归一化进度判断是否应开启或关闭检测窗口。
        /// </summary>
        /// <param name="progress">当前动画归一化进度 (0~1)</param>
        private void UpdateDetectionWindow(float progress)
        {
            bool shouldBeOpen = progress >= _currentAttack.HitWindowStart && progress < _currentAttack.HitWindowEnd;

            if (shouldBeOpen && !_isDetecting)
            {
                // 进入检测窗口，清空上一轮命中记录
                _isDetecting = true;
                _hitTargetIds.Clear();
            }
            else if (!shouldBeOpen && _isDetecting)
            {
                _isDetecting = false;
            }
        }

        /// <summary>
        /// 强制关闭检测窗口（动画结束时调用）。
        /// </summary>
        private void CloseDetectionIfOpen()
        {
            if (_isDetecting)
            {
                _isDetecting = false;
            }
        }

        /// <summary>
        /// 使用默认攻击索引 0 发动攻击（供行为树无参反射调用）。
        /// </summary>
        public void StartAttack()
        {
            StartAttack(0);
        }

        /// <summary>
        /// 发动指定索引的攻击：播放攻击动画并重置冷却。
        /// </summary>
        /// <param name="attackIndex">攻击数据在 AttackDataList 中的索引</param>
        public void StartAttack(int attackIndex)
        {
            if (AnimationSystem == null) return;

            // 边界检查
            if (attackIndex < 0 || attackIndex >= AnimationSystem.AttackCount)
            {
                UnityEngine.Debug.LogWarning($"[EnemyCombat] 无效的攻击索引: {attackIndex}");
                return;
            }

            _currentAttack = AnimationSystem.GetAttackData(attackIndex);
            _currentAnimState = AnimationSystem.PlayAttack(attackIndex);
            _cooldownTimer = AttackCooldown;
            _isAttacking = true;
        }

        /// <summary>
        /// 随机触发一个攻击动画，伪实现敌人AI攻击。
        /// </summary>
        public void StartRandomAttack()
        {
            if (AnimationSystem == null) return;
            StartAttack(Random.Range(0, AnimationSystem.AttackCount));
        }


        /// <summary>
        /// 判断攻击是否就绪（冷却完毕且当前不在攻击中）。
        /// </summary>
        /// <returns>true 表示可以发动下一次攻击</returns>
        public bool IsAttackReady()
        {
            return _cooldownTimer <= 0f && !_isAttacking;
        }

        /// <summary>
        /// 查询当前是否正在攻击中（供行为树条件节点调用）。
        /// </summary>
        /// <returns>true 表示攻击动画正在播放</returns>
        public bool IsAttacking()
        {
            return _isAttacking;
        }
    }
}
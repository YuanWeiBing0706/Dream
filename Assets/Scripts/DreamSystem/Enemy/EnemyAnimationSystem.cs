using Animancer;
using Sirenix.OdinInspector;
using Struct;
using UnityEngine;
using UnityEngine.AI;

namespace DreamSystem.Enemy
{
    /// <summary>
    /// 敌人动画系统（纯粹的被动动画播放器，持有所有动画资源）。
    /// <para>作为纯展示层 (View)，统一管理所有敌人动画片段。外部逻辑系统通过方法名/索引告知播放什么动画。</para>
    /// </summary>
    public class EnemyAnimationSystem : MonoBehaviour
    {
        [SerializeField]
        private AnimancerComponent Animancer;

        [SerializeField]
        private NavMeshAgent NavMeshAgent;

        [Header("Locomotion")]
        [SerializeField]
        private ClipTransition IdleClip;
        [SerializeField]
        private ClipTransition MoveClip;

        [Header("Attack")]
        [SerializeField]
        private EnemyAttackData[] AttackDataList;

        [SerializeField]
        [DrawWithUnity]
        private ClipTransition HitClip;
        [SerializeField]
        [DrawWithUnity]
        private ClipTransition DeadClip;

        /// <summary>
        /// 是否正在播放高优先级的一次性动作（攻击、受击、死亡等），
        /// 当为 true 时，外部的 Locomotion 请求（PlayIdle/PlayMove）会被忽略，防止打断播放。
        /// </summary>
        private bool _isActionPlaying;
        private AnimancerState _currentActionState;

        // ==================== 状态属性 ====================

        /// <summary>
        /// 是否正在播放高优先级的一次性动作（供行为树的 WaitUntilHitFinished 节点检查动画是否播完）
        /// </summary>
        public bool IsActionPlaying => _isActionPlaying;

        /// <summary>
        /// 攻击数据总数（供 CombatSystem 做随机索引和边界检查）。
        /// </summary>
        public int AttackCount => AttackDataList != null ? AttackDataList.Length : 0;

        private void Awake()
        {
            if (Animancer == null) Animancer = GetComponent<AnimancerComponent>();
            if (NavMeshAgent == null) NavMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            PlayIdle();
        }

        private void Update()
        {
            // 唯一职责：检查高优先级动画是否播完，播完后自动归还控制权
            if (_isActionPlaying && _currentActionState != null && _currentActionState.NormalizedTime >= 1f)
            {
                _isActionPlaying = false;
                _currentActionState = null;
                PlayIdle();
            }
        }

        // ==================== Locomotion ====================

        /// <summary>
        /// 播放待机动画（由 MoveSystem.Stop / 行为树调用）。
        /// 如果正在播放高优先级动画则忽略。
        /// </summary>
        public void PlayIdle()
        {
            if (_isActionPlaying) return;
            if (IdleClip != null && IdleClip.Clip != null) Animancer.Play(IdleClip);
        }

        /// <summary>
        /// 播放移动动画（由 MoveSystem.ChasePlayer / 行为树调用）。
        /// 如果正在播放高优先级动画则忽略。
        /// </summary>
        public void PlayMove()
        {
            if (_isActionPlaying) return;
            if (MoveClip != null && MoveClip.Clip != null) Animancer.Play(MoveClip);
        }

        // ==================== Attack ====================

        /// <summary>
        /// 获取指定索引的攻击数据（供 CombatSystem 读取伤害值、HitBox 索引、判定窗口等）。
        /// </summary>
        public EnemyAttackData GetAttackData(int index)
        {
            return AttackDataList[index];
        }

        /// <summary>
        /// 播放指定索引的攻击动画（由 CombatSystem / 行为树调用）。
        /// </summary>
        /// <returns>返回 AnimancerState，供 CombatSystem 读取进度来开关伤害判定窗口</returns>
        public AnimancerState PlayAttack(int index)
        {
            if (AttackDataList == null || index < 0 || index >= AttackDataList.Length) return null;

            var clip = AttackDataList[index].Clip;
            if (clip == null || clip.Clip == null) return null;

            _isActionPlaying = true;
            _currentActionState = Animancer.Play(clip);
            return _currentActionState;
        }

        // ==================== Passivity ====================
        
        
        /// <summary>
        /// 播放受击动画（由行为树受击分支调用）。
        /// </summary>
        public void PlayHit()
        {
            if (HitClip == null || HitClip.Clip == null) return;

            // 立刻停止移动
            if (NavMeshAgent != null) NavMeshAgent.ResetPath();

            _isActionPlaying = true;
            _currentActionState = Animancer.Play(HitClip);
        }

        /// <summary>
        /// 播放死亡动画（终态，由行为树死亡分支调用）。
        /// </summary>
        public void PlayDead()
        {
            if (DeadClip == null || DeadClip.Clip == null) return;

            // 立刻停止移动
            if (NavMeshAgent != null) NavMeshAgent.ResetPath();

            _isActionPlaying = true;
            _currentActionState = Animancer.Play(DeadClip);
            // 死亡是终态，不需要归还控制权
        }
    }
}
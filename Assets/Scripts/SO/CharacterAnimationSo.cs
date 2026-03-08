using System;
using UnityEngine;
using Animancer;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace SO
{
    [CreateAssetMenu(fileName = "NewCharacterAnimData", menuName = "SO/Character Animation Data")]
    public class CharacterAnimationSo : SerializedScriptableObject
    {
        // ============================================
        // 移动系统 (Locomotion)
        // ============================================
        [TabGroup("Animations", "Locomotion")]
        [Tooltip("自由视角下的移动 (Idle -> Walk -> Run -> Sprint)")]
        [DrawWithUnity]
        public LinearMixerTransition FreeMoveMixer;

        [TabGroup("Animations", "Locomotion")]
        [Tooltip("锁定视角下的战斗步伐 (前后左右)")]
        [DrawWithUnity]
        public MixerTransition2D LockedMoveMixer;

        // ============================================
        // 跳跃系统 (Air)
        // ============================================
        [TabGroup("Animations", "Air")]
        [DrawWithUnity]
        public ClipTransition JumpStart;
        [TabGroup("Animations", "Air")]
        [DrawWithUnity]
        public ClipTransition Fall;
        [TabGroup("Animations", "Air")]
        [DrawWithUnity]
        public ClipTransition Land;


        // ============================================
        // 战斗系统 (Combat)
        // ============================================
        [TabGroup("Animations", "Combat")]
        [LabelText("轻攻击连招")]
        public List<AttackClipData> LightAttacks = new List<AttackClipData>();

        [TabGroup("Animations", "Combat")]
        [LabelText("重攻击连招")]
        public List<AttackClipData> HeavyAttacks = new List<AttackClipData>();

        [TabGroup("Animations", "Combat")]
        [LabelText("跳跃攻击")]
        public List<AttackClipData> JumpAttacks = new List<AttackClipData>();

        [TabGroup("Animations", "Combat")]
        [Searchable]
        [Tooltip("通过 Key 查找的特殊动作")]
        public Dictionary<string, SkillAttackData> Skills = new Dictionary<string, SkillAttackData>();

        [TabGroup("Animations", "Combat")]
        [DrawWithUnity]
        public ClipTransition Defend;

        [TabGroup("Animations", "Combat")]
        [DrawWithUnity]
        public ClipTransition DefendHit;

        // ============================================
        // 闪避系统 (Evasion)
        // ============================================
        [TabGroup("Animations", "Evasion")]
        [DrawWithUnity]
        public ClipTransition Dash;
        [TabGroup("Animations", "Evasion")]
        [DrawWithUnity]
        public DirectionalSet RollAnimations;

        // ============================================
        // 状态系统 (Status)
        // ============================================
        [TabGroup("Animations", "Status")]
        [DrawWithUnity]
        public ClipTransition GetHit;
        [TabGroup("Animations", "Status")]
        [DrawWithUnity]
        public ClipTransition Die;
        [TabGroup("Animations", "Status")]
        [DrawWithUnity]
        public ClipTransition Revive;
        [TabGroup("Animations", "Status")]
        [DrawWithUnity]
        public ClipTransition Victory;

        [TabGroup("Animations", "Status")]
        [DrawWithUnity]
        public ClipTransition Dizzy;

        /// <summary>
        /// 通过 Key 获取技能攻击数据。
        /// </summary>
        /// <param name="key">技能键名</param>
        /// <returns>技能攻击数据，不存在则返回 null</returns>
        public SkillAttackData GetSkill(string key)
        {
            if (Skills.TryGetValue(key, out var data))
            {
                return data;
            }
            return null;
        }
    }

    /// <summary>
    /// 攻击动画数据，包含动画片段和伤害窗口配置。
    /// </summary>
    [Serializable]
    public class AttackClipData
    {
        [DrawWithUnity]
        [LabelText("动画")]
        public ClipTransition Clip;

        [PropertyRange(0f, 1f)]
        [LabelText("伤害开始 (0-1)")]
        [Tooltip("伤害检测开始的动画进度 (0 = 动画开始, 1 = 动画结束)")]
        public float HitWindowStart = 0f;

        [PropertyRange(0f, 1f)]
        [LabelText("伤害结束 (0-1)")]
        [Tooltip("伤害检测结束的动画进度")]
        public float HitWindowEnd = 0.9f;

        /// 隐式转换为 ClipTransition，方便兼容现有代码
        public static implicit operator ClipTransition(AttackClipData data) => data?.Clip;
    }

    /// <summary>
    /// 技能攻击数据，包含动画和伤害窗口配置。
    /// </summary>
    [Serializable]
    public class SkillAttackData
    {
        [DrawWithUnity]
        [LabelText("动画")]
        public ClipTransition Clip;

        [PropertyRange(0f, 1f)]
        [LabelText("伤害开始 (0-1)")]
        public float HitWindowStart = 0f;

        [PropertyRange(0f, 1f)]
        [LabelText("伤害结束 (0-1)")]
        public float HitWindowEnd = 0.9f;

        /// 隐式转换为 ClipTransition
        public static implicit operator ClipTransition(SkillAttackData data) => data?.Clip;
    }

    [Serializable]
    public struct DirectionalSet
    {
        [DrawWithUnity]
        public ClipTransition Forward;
        [DrawWithUnity]
        public ClipTransition Back;
        [DrawWithUnity]
        public ClipTransition Left;
        [DrawWithUnity]
        public ClipTransition Right;

        /// <summary>
        /// 根据输入方向获取对应的翻滚动画。
        /// </summary>
        /// <param name="input">2D 输入方向</param>
        /// <returns>对应方向的动画</returns>
        public ClipTransition GetClipByDirection(Vector2 input)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return input.x > 0 ? Right : Left;
            }
            else
            {
                return input.y >= 0 ? Forward : Back;
            }
        }
    }
}
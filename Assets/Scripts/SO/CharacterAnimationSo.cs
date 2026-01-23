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
        [DrawWithUnity]
        public ClipTransition[] Attacks;

        [TabGroup("Animations", "Combat")]
        [Searchable]
        [Tooltip("通过 Key 查找的特殊动作")]
        public Dictionary<string, SkillTransition> Skills = new Dictionary<string, SkillTransition>();

        [TabGroup("Animations", "Combat")]
        [DrawWithUnity]
        public ClipTransition Defend;

        // ... (Evasion, Status 保持你现在的样子) ...
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

        // 【修改点 3】：添加辅助方法供外部获取，隐藏 Wrapper 的存在
        public ClipTransition GetSkill(string key)
        {
            if (Skills.TryGetValue(key, out var wrapper))
            {
                // 如果 wrapper 不为空，返回里面的 Transition，否则返回 null
                return wrapper?.Transition;
            }
            return null;
        }
    }

    // 【修改点 2】: 新增包装类
    [Serializable]
    public class SkillTransition
    {
        [DrawWithUnity]
        public ClipTransition Transition;

        // 方便隐式转换
        public static implicit operator ClipTransition(SkillTransition wrapper) => wrapper?.Transition;
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
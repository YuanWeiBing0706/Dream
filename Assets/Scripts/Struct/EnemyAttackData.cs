using System;
using Animancer;
using Sirenix.OdinInspector;
using UnityEngine;
namespace Struct
{
    /// <summary>
    /// 单次攻击的配置数据。
    /// <para>每个攻击动画可以指定不同的激活 HitBox、伤害窗口和伤害值。</para>
    /// </summary>
    [Serializable]
    public struct EnemyAttackData
    {
        /// 攻击动画
        [DrawWithUnity]
        public ClipTransition clip;

        /// 本次攻击激活的 HitBox 索引（对应 _enemyHitBoxes 数组下标）
        public int[] activeHitBoxIndices;

        /// 伤害窗口开始时机（动画进度百分比，0~1）
        [Range(0f, 1f)]
        public float hitWindowStart;

        /// 伤害窗口结束时机（动画进度百分比，0~1）
        [Range(0f, 1f)]
        public float hitWindowEnd;

        /// 攻击伤害值
        public float damage;
    }
}
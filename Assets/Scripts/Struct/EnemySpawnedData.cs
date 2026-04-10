using UnityEngine;

namespace Struct
{
    /// <summary>
    /// 敌人生成事件的数据包。
    /// <para>由 WaveManager 生成怪物后发布，EnemyInjuriedSystem 订阅后完成伤害系统注册。</para>
    /// </summary>
    public struct EnemySpawnedData
    {
        /// 被生成的敌人 GameObject
        public GameObject EnemyGo;

        /// 与 CharacterStatsConfig 中 characterId 对应的唯一标识（如 "slime"、"dragon"）
        public string CharacterId;
    }
}

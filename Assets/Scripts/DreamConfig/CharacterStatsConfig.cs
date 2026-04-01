using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DreamAttribute;
using DreamManager;
using Function;
using UnityEngine;

namespace DreamConfig
{
    /// <summary>
    /// 角色属性配置表（玩家 + 所有敌人共用）。
    /// <para>CSV 格式：CharacterId,Health,Shield,Attack,Defense,Speed</para>
    /// </summary>
    [Config]
    public class CharacterStatsConfig : Config
    {
        private readonly Dictionary<string, CharacterStatsData> _characterStatsDir = new Dictionary<string, CharacterStatsData>();
        
        private readonly List<CharacterStatsData> _allCharacterStatsList = new List<CharacterStatsData>();
        
        public override UniTask LoadConfig(ResourcesManager resourcesManager)
        {
            var textAsset = resourcesManager.LoadAsset<TextAsset>(nameof(CharacterStatsConfig));
            var data = CsvHelper.ReadCsv(textAsset);

            for (int i = 0; i < data.Count; i++)
            {
                var statsData = new CharacterStatsData
                {
                    characterId = data[i][0],
                    baseMaxHealth = float.Parse(data[i][1]),
                    baseShield = float.Parse(data[i][2]),
                    baseAttack = float.Parse(data[i][3]),
                    baseDefense = float.Parse(data[i][4]),
                    baseSpeed = float.Parse(data[i][5])
                };
                if (!_characterStatsDir.TryAdd(statsData.characterId, statsData)) {
                    UnityEngine.Debug.LogError($"[LevelConfig] 发现重复等级ID: {statsData.characterId}");
                    continue;
                }
                _allCharacterStatsList.Add(statsData);
            }

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 按角色 ID 查找属性数据。
        /// </summary>
        public CharacterStatsData this[string characterId] => _characterStatsDir[characterId];

        /// <summary>
        /// 安全查找，不存在返回 false。
        /// </summary>
        public bool TryGet(string characterId, out CharacterStatsData data) => _characterStatsDir.TryGetValue(characterId, out data);
        
        public List<CharacterStatsData> GetAllCharacterStatsList() => _allCharacterStatsList;
    }
    
    /// <summary>
    /// 角色基础属性配置数据。
    /// 仅用于游戏启动或实体生成时的首次数据加载，不参与运行时的动态计算。
    /// </summary>
    public struct CharacterStatsData
    {
        /// <summary>
        /// 角色或怪物的唯一标识符（如 "player_01", "slime_01"）
        /// 用于将配置数据与具体的预制体或实例进行绑定。
        /// </summary>
        public string characterId;

        /// <summary>
        /// 基础最大生命值。
        /// 对应运行时的最大生命值上限（MaxHealth），而非当前血量。
        /// </summary>
        public float baseMaxHealth;

        /// <summary>
        /// 基础最大护盾容量 / 初始护盾值。
        /// 如果游戏机制中护盾没有上限概念（可以无限叠加），此字段表示“出生自带的固定护盾值”；
        /// 如果护盾有上限概念，此字段表示护盾条的上限。
        /// </summary>
        public float baseShield;

        /// <summary>
        /// 基础攻击力。
        /// 没有任何 Buff 和装备加成时的初始物理/魔法伤害基数。
        /// </summary>
        public float baseAttack;

        /// <summary>
        /// 基础防御力。
        /// 用于在伤害结算公式中进行减伤计算的基础值。
        /// </summary>
        public float baseDefense;

        /// <summary>
        /// 基础速度。
        /// 影响移动速度或攻击频率的初始值。
        /// </summary>
        public float baseSpeed;
    }
}
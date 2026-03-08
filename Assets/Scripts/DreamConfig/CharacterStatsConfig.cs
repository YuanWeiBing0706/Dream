using System.Collections.Generic;
using Attribute;
using Cysharp.Threading.Tasks;
using DreamManager;
using Function;

namespace DreamConfig
{
    /// <summary>
    /// 角色属性配置表（玩家 + 所有敌人共用）。
    /// <para>CSV 格式：CharacterId,Health,Shield,Attack,Defense,Speed</para>
    /// </summary>
    [Config]
    public class CharacterStatsConfig : Config
    {
        private readonly Dictionary<string, CharacterStatsData> _dic = new();

        public override UniTask LoadConfig(ResourcesManager resourcesManager)
        {
            var textAsset = resourcesManager.LoadAsset<UnityEngine.TextAsset>(nameof(CharacterStatsConfig));
            var data = CsvHelper.ReadCsv(textAsset);

            for (int i = 0; i < data.Count; i++)
            {
                var statsData = new CharacterStatsData
                {
                    characterId = data[i][0],
                    health = float.Parse(data[i][1]),
                    shield = float.Parse(data[i][2]),
                    attack = float.Parse(data[i][3]),
                    defense = float.Parse(data[i][4]),
                    speed = float.Parse(data[i][5])
                };
                _dic.Add(statsData.characterId, statsData);
            }

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 按角色 ID 查找属性数据。
        /// </summary>
        public CharacterStatsData this[string characterId] => _dic[characterId];

        /// <summary>
        /// 安全查找，不存在返回 false。
        /// </summary>
        public bool TryGet(string characterId, out CharacterStatsData data) => _dic.TryGetValue(characterId, out data);
    }

    /// <summary>
    /// 单个角色的基础属性数据。
    /// </summary>
    public struct CharacterStatsData
    {
        public string characterId;
        public float health;
        public float shield;
        public float attack;
        public float defense;
        public float speed;
    }
}
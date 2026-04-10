using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DreamAttribute;
using DreamManager;
using Function;
using UnityEngine;

namespace DreamConfig
{
    /// <summary>
    /// 关卡配置表。
    /// <para>CSV 格式：LevelId,LevelName,ExperienceReward,DropGroupId,MonsterList,IsBoss,Description</para>
    /// <para>MonsterList 格式：id:count|id:count（count 决定该怪物在随机池中的份额，影响出现概率）</para>
    /// <para>例如 "OrcPADefault:3|SkeletonPADefault:2" → 池子展开为 [OrcPADefault,OrcPADefault,OrcPADefault,SkeletonPADefault,SkeletonPADefault]</para>
    /// </summary>
    [Config]
    public class LevelConfig : Config
    {
        private readonly Dictionary<string, LevelData> _levelDir = new();
        private readonly List<LevelData> _allLevelList = new List<LevelData>();

        public override UniTask LoadConfig(ResourcesManager resourcesManager)
        {
            var textAsset = resourcesManager.LoadAsset<TextAsset>(nameof(LevelConfig));
            var data = CsvHelper.ReadCsv(textAsset);

            // CsvHelper 已跳过标题行，从 i=0 开始遍历数据行
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].Length < 7 || string.IsNullOrWhiteSpace(data[i][0])) continue;

                var levelData = new LevelData
                {
                    levelId          = data[i][0],
                    levelName        = data[i][1],
                    experienceReward = int.Parse(data[i][2]),
                    dropGroupId      = data[i][3],
                    enemyPool        = ParseMonsterPool(data[i][4]),
                    isBoss           = bool.Parse(data[i][5]),
                    description      = data[i][6]
                };

                if (!_levelDir.TryAdd(levelData.levelId, levelData))
                {
                    Debug.LogError($"[LevelConfig] 发现重复关卡ID: {levelData.levelId}");
                    continue;
                }
                _allLevelList.Add(levelData);
            }

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 解析 "id:count|id:count" 格式，展开为加权随机池。
        /// 例如 "OrcPADefault:3|SkeletonPADefault:2" → [OrcPADefault, OrcPADefault, OrcPADefault, SkeletonPADefault, SkeletonPADefault]
        /// </summary>
        private static string[] ParseMonsterPool(string monsterListStr)
        {
            if (string.IsNullOrEmpty(monsterListStr)) return System.Array.Empty<string>();

            var pool = new List<string>();
            foreach (var entry in monsterListStr.Split('|'))
            {
                var trimmed = entry.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                var parts = trimmed.Split(':');
                string monsterId = parts[0].Trim();
                int count = (parts.Length > 1 && int.TryParse(parts[1], out int c) && c > 0) ? c : 1;

                for (int k = 0; k < count; k++)
                    pool.Add(monsterId);
            }
            return pool.ToArray();
        }

        public LevelData this[string levelId] => _levelDir[levelId];

        public bool TryGet(string levelId, out LevelData data) => _levelDir.TryGetValue(levelId, out data);

        public List<LevelData> GetAllLevels() => _allLevelList;
    }

    public struct LevelData
    {
        /// 关卡 ID（如 "level_01"）
        public string levelId;

        /// 关卡名称
        public string levelName;

        /// 奖励经验
        public int experienceReward;

        /// 掉落组 ID（关联 DropEntryConfig）
        public string dropGroupId;

        /// 展开后的怪物随机池（供 WaveManager.StartWave/StartBossWave 使用）
        public string[] enemyPool;

        /// 是否为 Boss 关（true = 只打1只 Boss，false = 打2波普通怪）
        public bool isBoss;

        /// 关卡描述
        public string description;
    }
}

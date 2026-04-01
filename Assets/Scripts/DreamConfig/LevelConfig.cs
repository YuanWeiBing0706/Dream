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
    /// <para>CSV 格式：LevelId,LevelName,ExperienceReward,DropGroupId,MonsterList,Description</para>
    /// <para>MonsterList 用 | 分隔多个敌人 ID。</para>
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

            // 从 i = 1 开始遍历，跳过第一行表头
            for (int i = 1; i < data.Count; i++)
            {
                // 如果空行或者数据不足，跳过
                if (data[i].Length < 6 || string.IsNullOrWhiteSpace(data[i][0])) continue;

                var enemyListStr = data[i][4]; // 第5列是 MonsterList
                var enemies = string.IsNullOrEmpty(enemyListStr)
                    ? new string[0]
                    : enemyListStr.Split('|');

                var levelData = new LevelData
                {
                    levelId = data[i][0],
                    levelName = data[i][1],
                    experienceReward = int.Parse(data[i][2]),
                    dropGroupId = data[i][3],
                    enemyList = enemies,
                    description = data[i][5]
                };
                if (!_levelDir.TryAdd(levelData.levelId, levelData)) {
                    UnityEngine.Debug.LogError($"[LevelConfig] 发现重复等级ID: {levelData.levelId}");
                    continue;
                }
                _allLevelList.Add(levelData);
            }

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 按关卡 ID 查找。
        /// </summary>
        public LevelData this[string levelId] => _levelDir[levelId];

        public bool TryGet(string levelId, out LevelData data) => _levelDir.TryGetValue(levelId, out data);

        /// <summary>
        /// 获取所有关卡列表（按 CSV 顺序）。
        /// </summary>
        public List<LevelData> GetAllLevels() => _allLevelList;
    }

    /// <summary>
    /// 单个关卡的配置数据。
    /// </summary>
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

        /// 该关需要刷出的敌人 ID 列表
        public string[] enemyList;

        /// 关卡描述
        public string description;
    }
}

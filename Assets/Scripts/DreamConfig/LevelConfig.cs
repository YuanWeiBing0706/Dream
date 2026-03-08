using System.Collections.Generic;
using Attribute;
using Cysharp.Threading.Tasks;
using DreamManager;
using Function;

namespace DreamConfig
{
    /// <summary>
    /// 关卡配置表。
    /// <para>3 大关 × 3 小关 = 9 关。</para>
    /// <para>CSV 格式：LevelId,MajorStage,MinorStage,EnemyList,RewardGold,DropGroupId</para>
    /// <para>EnemyList 用 | 分隔多个敌人 ID，DropGroupId 关联 DropConfig 的掉落组。</para>
    /// </summary>
    [Config]
    public class LevelConfig : Config
    {
        private readonly Dictionary<string, LevelData> _dic = new();
        private readonly List<LevelData> _allLevels = new();

        public override UniTask LoadConfig(ResourcesManager resourcesManager)
        {
            var textAsset = resourcesManager.LoadAsset<UnityEngine.TextAsset>(nameof(LevelConfig));
            var data = CsvHelper.ReadCsv(textAsset);

            for (int i = 0; i < data.Count; i++)
            {
                var enemyListStr = data[i][3];
                var enemies = string.IsNullOrEmpty(enemyListStr)
                    ? new string[0]
                    : enemyListStr.Split('|');

                var levelData = new LevelData
                {
                    levelId = data[i][0],
                    majorStage = int.Parse(data[i][1]),
                    minorStage = int.Parse(data[i][2]),
                    enemyList = enemies,
                    rewardGold = int.Parse(data[i][4]),
                    dropGroupId = data[i][5]
                };
                _dic.Add(levelData.levelId, levelData);
                _allLevels.Add(levelData);
            }

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 按关卡 ID 查找。
        /// </summary>
        public LevelData this[string levelId] => _dic[levelId];

        public bool TryGet(string levelId, out LevelData data) => _dic.TryGetValue(levelId, out data);

        /// <summary>
        /// 获取所有关卡列表（按 CSV 顺序）。
        /// </summary>
        public List<LevelData> GetAllLevels() => _allLevels;
    }

    /// <summary>
    /// 单个关卡的配置数据。
    /// </summary>
    public struct LevelData
    {
        /// 关卡 ID（如 "1-1"）
        public string levelId;

        /// 大关编号（1~3）
        public int majorStage;

        /// 小关编号（1~3）
        public int minorStage;

        /// 该关需要刷出的敌人 ID 列表
        public string[] enemyList;

        /// 通关奖励金币
        public int rewardGold;

        /// 掉落组 ID（关联 DropConfig）
        public string dropGroupId;
    }
}

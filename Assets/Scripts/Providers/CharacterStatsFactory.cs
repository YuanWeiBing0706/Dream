using Cysharp.Threading.Tasks;
using DreamConfig;
using DreamManager;
using DreamSystem.Damage.Stat;
using Function.Initialize;
using Struct;
using UnityEngine;

namespace Providers
{
    /// <summary>
    /// 角色属性工厂。
    /// <para>根据 characterId 从 CharacterStatsConfig（CSV 配置表）读取基础属性，</para>
    /// <para>创建并返回一个全新的 CharacterStats 实例。</para>
    /// <para>玩家和敌人共用此工厂；工厂本身是单例，但产出的 CharacterStats 是各自独立的新对象。</para>
    /// </summary>
    public class CharacterStatsFactory
    {
        private readonly ResourcesManager _resources;

        public CharacterStatsFactory(ResourcesManager resources)
        {
            _resources = resources;
        }

        /// <summary>
        /// 根据 characterId 创建对应的 CharacterStats 实例。
        /// </summary>
        /// <param name="characterId">角色标识（与 CSV 表中的 characterId 列对应，如 "player"、"slime"、"dragon"）</param>
        /// <returns>初始化完毕的 CharacterStats 新实例</returns>
        public CharacterStats Create(string characterId)
        {
            var config = _resources.GetConfig<CharacterStatsConfig>();
            if (config == null)
            {
                Debug.LogError("[CharacterStatsFactory] CharacterStatsConfig 未加载，请确认 ResourcesManager 已完成初始化！");
                return new CharacterStats();
            }

            if (!config.TryGet(characterId, out var data))
            {
                Debug.LogError($"[CharacterStatsFactory] 找不到角色配置: {characterId}，将使用默认值");
                return new CharacterStats();
            }

            var initData = new CharacterStatsInitData
            {
                baseHealth = data.baseMaxHealth,
                baseShield = data.baseShield,
                baseAttack = data.baseAttack,
                baseDefense = data.baseDefense,
                baseSpeed = data.baseSpeed
            };

            return new CharacterStats(initData);
        }
        
        
        // public UniTask  AsyncStart()
        // {
        //     var config = _resources.GetConfig<CharacterStatsConfig>();
        //     if (config == null)
        //     {
        //         Debug.LogError("[CharacterStatsFactory] CharacterStatsConfig 未加载，请确认 ResourcesManager 已完成初始化！");
        //         return new CharacterStats();
        //     }
        //
        //     if (!config.TryGet(characterId, out var data))
        //     {
        //         Debug.LogError($"[CharacterStatsFactory] 找不到角色配置: {characterId}，将使用默认值");
        //         return new CharacterStats();
        //     }
        //
        //     var initData = new CharacterStatsInitData
        //     {
        //         BaseHealth = data.baseMaxHealth,
        //         BaseShield = data.baseShield,
        //         BaseAttack = data.baseAttack,
        //         BaseDefense = data.baseDefense,
        //         BaseSpeed = data.baseSpeed
        //     };
        //
        //     return new CharacterStats(initData);
        // }
    }
}
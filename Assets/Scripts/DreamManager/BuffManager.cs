using System;
using System.Collections.Generic;
using System.Reflection;
using DreamAttribute;
using DreamConfig;
using DreamSystem.Damage.Buff;
using DreamSystem.Damage.Buff.Data;
using Enum.Buff;
using Interface;
using VContainer.Unity;

namespace DreamManager
{
    /// <summary>
    /// Buff 逻辑工厂（总装配厂）。
    /// <para>通过反射扫描所有标记了 BuffLogicAttribute 的 BuffBaseLogic 子类，注册到构造器映射。</para>
    /// <para>根据 BuffLogicType 创建对应的 BuffBaseLogic 实例，并配合 Config 打包 Data 数据。</para>
    /// </summary>
    public class BuffManager : IStartable
    {
        readonly ResourcesManager _resources;
        // 【修正】彻底摒弃容易混淆的 BuffBaseData，统一改为代表业务逻辑的基类 BuffBaseLogic
        private static Dictionary<BuffLogicType, Func<BuffBaseLogic>> _constructorMap = new Dictionary<BuffLogicType, Func<BuffBaseLogic>>();

        public BuffManager(ResourcesManager resources)
        {
            _resources = resources;
        }
        
        /// <summary>
        /// VContainer 启动时自动调用：反射扫描程序集，注册所有标记了 BuffLogicAttribute 的类。
        /// </summary>
        public void Start()
        {
            // 【修正】反射寻找的基类也必须是 BuffBaseLogic
            var assembly = Assembly.GetAssembly(typeof(BuffBaseLogic));
            
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract)
                {
                    continue;
                }

                if (!typeof(BuffBaseLogic).IsAssignableFrom(type))
                {
                    continue;
                }

                var attr = type.GetCustomAttribute<BuffLogicAttribute>();
                
                if (attr == null)
                {
                    continue;
                }
                
                _constructorMap[attr.logicType] = CreateConstructor(type);
            }
            
            UnityEngine.Debug.Log($"[BuffManager] 成功注册了 {_constructorMap.Count} 种 Buff 逻辑！");
        }

        private Func<BuffBaseLogic> CreateConstructor(Type type)
        {
            return () => Activator.CreateInstance(type) as BuffBaseLogic;
        }

        private BuffBaseLogic CreateBuffBase(BuffLogicType buffType)
        {
            if (!_constructorMap.TryGetValue(buffType, out var ctor))
            {
                UnityEngine.Debug.LogError($"[BuffManager] BuffLogic 未找到，请检查是否贴了标签: {buffType}");
                return null;
            }

            return ctor();
        }

        /// <summary>
        /// 根据 Buff GUID 获取配置数据和逻辑实例。
        /// </summary>
        public (BuffBaseData data, BuffBaseLogic logic) GetBuff(string buffGuid)
        {
            var buffConfig= _resources.GetConfig<BuffConfig>();
            if (string.IsNullOrWhiteSpace(buffGuid))
            {
                UnityEngine.Debug.LogError("[BuffManager] buffGuid 为空，无法创建 Buff。");
                return (null, null);
            }
            buffGuid = buffGuid.Trim();
            // 1. 查图纸
            if (!buffConfig.TryGet(buffGuid, out BuffData config))
            {
                UnityEngine.Debug.LogError($"[BuffManager] 找不到 Buff 配置: {buffGuid}");
                return (null, null);
            }

            // 2. 按逻辑类型打包对应的 Data 纸箱
            BuffBaseData data;
            switch (config.logicType)
            {
                // 属性修改 & 属性转化：共用 AttributeBuffData（含 buffEntryDataList）
                case BuffLogicType.ModifyAttribute:
                case BuffLogicType.ConvertStat:
                case BuffLogicType.ConvertResourceToStat:
                    data = new AttributeBuffData
                    {
                        duration          = config.duration,
                        maxStack          = config.maxStack,
                        stackType         = config.stackType,
                        buffEntryDataList = config.buffEntryDataList
                    };
                    break;

                // 控制锁定：解析 stringParam → ControlLock 枚举，填入 ControlLockBuffData
                case BuffLogicType.AddControlLock:
                    var controlLocks = ControlLock.None;
                    if (!string.IsNullOrEmpty(config.stringParam) && config.stringParam != "None")
                    {
                        if (System.Enum.TryParse<ControlLock>(config.stringParam, true, out var parsed))
                            controlLocks = parsed;
                        else
                            UnityEngine.Debug.LogWarning($"[BuffManager] 无法解析 ControlLock: '{config.stringParam}'，请检查 ControlLock 枚举是否有该值");
                    }
                    data = new ControlLockBuffData
                    {
                        duration     = config.duration,
                        maxStack     = config.maxStack,
                        stackType    = config.stackType,
                        controlLocks = controlLocks
                    };
                    break;

                // 标记挂载 & 其他（含 TriggerOnEvent）：BasicBuffData
                default:
                    data = new BasicBuffData
                    {
                        duration  = config.duration,
                        maxStack  = config.maxStack,
                        stackType = config.stackType
                    };
                    break;
            }

            // stringParam 写入基类字段，所有逻辑均可通过 buffInstance.data.stringParam 访问
            data.stringParam = config.stringParam;

            // 【非常重要】：将基础 ID 赋给纸箱，这是 BuffSystem 识别 Buff 身份的唯一凭证！
            data.buffID = config.buffId;

            // 3. 找业务员 (Logic) —— 这里用的是你的高级字典反射！
            BuffBaseLogic logic = CreateBuffBase(config.logicType);

            // 4. 打包发货！
            return (data, logic);
        }
    }
}
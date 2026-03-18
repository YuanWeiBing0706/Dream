using System;
using System.Collections.Generic;
using System.Reflection;
using DreamAttribute;
using DreamConfig;
using DreamSystem.Damage.Buff;
using DreamSystem.Damage.Buff.Data;
using Enum.Buff;

namespace DreamManager
{
    /// <summary>
    /// Buff 逻辑工厂（总装配厂）。
    /// <para>通过反射扫描所有标记了 BuffLogicAttribute 的 BuffBaseLogic 子类，注册到构造器映射。</para>
    /// <para>根据 BuffLogicType 创建对应的 BuffBaseLogic 实例，并配合 Config 打包 Data 数据。</para>
    /// </summary>
    public class BuffManager
    {
        readonly ResourcesManager _resources;
        // 【修正】彻底摒弃容易混淆的 BuffBaseData，统一改为代表业务逻辑的基类 BuffBaseLogic
        private static Dictionary<BuffLogicType, Func<BuffBaseLogic>> _constructorMap = new Dictionary<BuffLogicType, Func<BuffBaseLogic>>();

        public BuffManager(ResourcesManager resources)
        {
            _resources = resources;
        }
        
        /// <summary>
        /// 初始化：反射扫描程序集，注册所有标记了 BuffLogicAttribute 的类。
        /// </summary>
        public void Init()
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
            // 1. 查图纸
            if (!buffConfig.TryGet(buffGuid, out BuffData config))
            {
                UnityEngine.Debug.LogError($"[BuffManager] 找不到 Buff 配置: {buffGuid}");
                // return (null, null);
            }

            // 2. 发纸箱 (Data) ！！！看这里，我们不用反射，直接 switch ！！！
            BuffBaseData data;
            switch (config.logicType)
            {
                // 如果是修改属性的，就给它专属的 AttributeBuffData
                case BuffLogicType.ModifyAttribute:
                    data = new AttributeBuffData 
                    { 
                        duration = config.duration, 
                        maxStack = config.maxStack,
                        buffEntryDataList = config.buffEntryDataList // 把图纸里的 list 塞进纸箱
                    };
                    break;
            
                // 如果是其他的普通类型，直接发白板纸箱
                default:
                    data = new BasicBuffData 
                    { 
                        duration = config.duration,
                        maxStack = config.maxStack
                    };
                    break;
            }

            // 【非常重要】：将基础 ID 赋给纸箱，这是 BuffSystem 识别 Buff 身份的唯一凭证！
            data.buffID = config.buffId;

            // 3. 找业务员 (Logic) —— 这里用的是你的高级字典反射！
            BuffBaseLogic logic = CreateBuffBase(config.logicType);

            // 4. 打包发货！
            return (data, logic);
        }
    }
}
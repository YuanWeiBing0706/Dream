using System;
using System.Collections.Generic;
using System.Reflection;
using Attribute;
using DreamSystem.Damage.Buff;
using Enum.Buff;
namespace DreamManager
{
    /// <summary>
    /// Buff 逻辑工厂。
    /// <para>通过反射扫描所有标记了 BuffLogicAttribute 的 BuffBase 子类，注册到构造器映射。</para>
    /// <para>根据 BuffType 创建对应的 BuffBase 实例。</para>
    /// </summary>
    public class BuffManager
    {
        private static Dictionary<BuffType, Func<BuffBase>> _constructorMap = new Dictionary<BuffType, Func<BuffBase>>();

        /// <summary>
        /// 初始化：反射扫描程序集，注册所有标记了 BuffLogicAttribute 的类。
        /// </summary>
        public void Init()
        {
            var assembly = Assembly.GetAssembly(typeof(BuffBase));
            
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract)
                {
                    continue;
                }

                if (!typeof(BuffBase).IsAssignableFrom(type))
                {
                    continue;
                }

                var attr = type.GetCustomAttribute<BuffLogicAttribute>();
                
                if (attr == null)
                {
                    continue;
                }

                _constructorMap[attr.buffType] = CreateConstructor(type);
            }
        }

        private Func<BuffBase> CreateConstructor(Type type)
        {
            return () => Activator.CreateInstance(type) as BuffBase;
        }

        private BuffBase CreateBuffBase(BuffType buffType)
        {
            if (!_constructorMap.TryGetValue(buffType, out var ctor))
            {
                UnityEngine.Debug.LogError($"BuffLogic not found: {buffType}");
                return null;
            }

            return ctor();
        }

        /// <summary>
        /// 根据 Buff GUID 获取配置数据和逻辑实例。
        /// <para>待实现：配合 BuffConfig 查表获取 BuffData，根据 BuffType 创建 BuffBase。</para>
        /// </summary>
        public (BuffData data, BuffBase logic) GetBuff(string buffGuid)
        {
            //配合查表
            return (null, null);
        }
    }
}
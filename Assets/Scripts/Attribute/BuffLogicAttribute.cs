using System;
using Enum.Buff;
namespace Attribute
{
    /// <summary>
    /// Buff 逻辑类型标记特性。
    /// <para>标记在 BuffBase 子类上，BuffManager 通过反射扫描此特性进行工厂注册。</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class BuffLogicAttribute : System.Attribute
    {
        /// 对应的 Buff 类型
        public BuffType buffType;

        public BuffLogicAttribute(BuffType buffType)
        {
            this.buffType = buffType;
        }
    }
}

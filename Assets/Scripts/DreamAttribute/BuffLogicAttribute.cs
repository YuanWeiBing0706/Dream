using System;
using Enum.Buff;
namespace DreamAttribute
{
    /// <summary>
    /// BuffLogic类型标记特性。
    /// <para>标记在 BuffBaseLogic 子类上</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class BuffLogicAttribute : Attribute
    {
        /// 对应的 Buff 类型
        public BuffLogicType logicType;

        public BuffLogicAttribute(BuffLogicType logicType)
        {
            this.logicType = logicType;
        }
    }
}

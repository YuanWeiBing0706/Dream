using System.Collections.Generic;
using Enum.Buff;
namespace DreamSystem.Damage.Buff.Data
{
    /// <summary>
    /// 带标签黑名单的 Buff 数据。
    /// <para>Buff 生效时会阻止特定标签的其他 Buff 被添加。</para>
    /// </summary>
    public class TagBlacklistBuffData : BuffBaseData
    {
        public List<BuffTag> blacklistTags = new List<BuffTag>();
    }
}
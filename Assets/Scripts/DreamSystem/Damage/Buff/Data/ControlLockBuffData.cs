using Enum.Buff;
using Interface;
namespace DreamSystem.Damage.Buff.Data
{
    /// <summary>
    /// 带控制锁定的 Buff 数据。
    /// <para>Buff 生效时会锁定玩家的指定行为（如移动、攻击、施法）。</para>
    /// </summary>
    public class ControlLockBuffData : BuffBaseData, IControlLockProvider
    {
        public ControlLock controlLocks = ControlLock.None;
        
        public ControlLock ControlLocks => controlLocks;
    }
}
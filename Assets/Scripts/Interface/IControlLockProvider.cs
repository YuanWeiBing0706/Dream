using DreamSystem.Damage.Buff;
using Enum.Buff;
namespace Interface
{
    /// <summary>
    /// 控制锁定提供者接口。
    /// </summary>
    public interface IControlLockProvider
    {
        ControlLock ControlLocks { get; }
    }
}
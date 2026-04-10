using DreamAttribute;
using DreamSystem.Damage.Buff.Data;
using Enum.Buff;
using UnityEngine;

namespace DreamSystem.Damage.Buff.Logic
{
    /// <summary>
    /// 控制锁定型 Buff 逻辑。
    /// <para>
    /// 生效时调用 <see cref="ControlGate.AddLocks"/>，向玩家施加行为禁锢；
    /// 移除时调用 <see cref="ControlGate.RemoveAllLocksFromSource"/> 精确撤销。
    /// </para>
    /// <para>
    /// 所需数据：<br/>
    /// - <c>ControlLockBuffData.controlLocks</c>：由 BuffManager 从 CSV 的
    ///   <c>stringParam</c> 列（如 "Dash"）解析而来。<br/>
    /// - <c>ControlGate</c>：从 owner 的 GameObject 上获取（玩家预制体需挂载该组件）。
    /// </para>
    /// </summary>
    [BuffLogic(BuffLogicType.AddControlLock)]
    public class AddControlLockBuffLogic : BuffBaseLogic
    {
        private ControlGate _controlGate;
        private ControlLock _locks;

        public override void OnApply()
        {
            if (buffInstance.data is ControlLockBuffData lockData)
                _locks = lockData.controlLocks;

            if (_locks == ControlLock.None)
            {
                UnityEngine.Debug.LogWarning("[AddControlLockBuffLogic] controlLocks 为 None，操作未被锁定");
                return;
            }

            // ControlGate 挂载在拥有者的 GameObject 上（玩家预制体）
            _controlGate = owner?.GameObject?.GetComponent<ControlGate>();
            if (_controlGate == null)
            {
                UnityEngine.Debug.LogWarning("[AddControlLockBuffLogic] 未找到 ControlGate 组件，控制锁定无效");
                return;
            }

            _controlGate.AddLocks(_locks, buffInstance);
        }

        public override void OnRemove()
        {
            _controlGate?.RemoveAllLocksFromSource(buffInstance);
        }
    }
}

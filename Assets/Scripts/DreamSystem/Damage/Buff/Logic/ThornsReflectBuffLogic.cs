using DreamAttribute;
using DreamSystem.Damage.Buff.Data;
using Enum.Buff;
using UDebug = UnityEngine.Debug;

namespace DreamSystem.Damage.Buff.Logic
{
    /// <summary>
    /// 反伤 Buff 逻辑（占位实现，核心防护结构已就绪）。
    /// <para>
    /// 当宿主受到伤害时，将一定比例的伤害反弹给攻击者。
    /// </para>
    /// <para>
    /// 【Stack Overflow 防护】静态 <c>_reflectDepth</c> 计数器限制反弹链最多 1 层：
    /// 玩家→怪A反弹→怪A的反弹深度=1，此时玩家自身反弹不再触发，链式死循环被切断。
    /// </para>
    /// <para>
    /// 【内存泄漏防护】EventManager 订阅须在 OnRemove 中取消。
    /// 当前版本事件订阅部分标记为 TODO，待 EventManager 可注入 BuffBaseLogic 后激活。
    /// </para>
    /// </summary>
    [BuffLogic(BuffLogicType.ThornsReflect)]
    public class ThornsReflectBuffLogic : BuffBaseLogic
    {
        /// 静态深度计数器：防止 A→B→A→B 的无限反弹递归
        private static int _reflectDepth;
        private const int MaxReflectDepth = 1;

        /// 反伤系数（来自 CSV buffEntryDataList[0].value，默认 0.3 = 30%）
        private float _reflectCoefficient = 0.3f;

        // TODO: EventManager 待通过 BuffBaseLogic.Initialize 或服务定位器注入
        // private DreamManager.EventManager _eventManager;

        public override void OnApply()
        {
            if (buffInstance?.data is AttributeBuffData attrData && attrData.buffEntryDataList?.Count > 0)
                _reflectCoefficient = attrData.buffEntryDataList[0].value;

            // TODO: 订阅全局伤害结果事件
            // _eventManager?.Subscribe<DamageResult>(Const.GameEvents.DAMAGE_RESULT, OnDamageResult);

            UDebug.Log($"[ThornsReflect] 反伤 Buff 生效，反弹系数={_reflectCoefficient * 100:F0}%（事件订阅待接入）");
        }

        public override void OnRemove()
        {
            // TODO: 取消订阅，防止游离引用导致内存泄漏
            // _eventManager?.Unsubscribe<DamageResult>(Const.GameEvents.DAMAGE_RESULT, OnDamageResult);
        }

        /// <summary>
        /// 处理伤害结果：若命中目标为宿主则向攻击者发起反弹伤害。
        /// 深度防护保证反弹链最多触发一次。
        /// </summary>
        private void OnDamageResult(Struct.DamageResult result)
        {
            // 深度防护：超过上限直接中止
            if (_reflectDepth >= MaxReflectDepth)
            {
                UDebug.Log("[ThornsReflect] 反弹深度已达上限，中止反弹链（Stack Overflow 防护生效）");
                return;
            }

            _reflectDepth++;
            try
            {
                // TODO: 计算反弹伤害并通过 EventManager 发布 DAMAGE_REQUEST
                // float reflectDamage = result.FinalDamage * _reflectCoefficient;
                // _eventManager?.Publish(Const.GameEvents.DAMAGE_REQUEST, new Struct.DamageRequest { ... });
                UDebug.Log("[ThornsReflect] 反弹伤害已计算（发布待接入）");
            }
            finally
            {
                _reflectDepth--;
            }
        }
    }
}

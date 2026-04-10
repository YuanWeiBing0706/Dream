using Const;
using DreamAttribute;
using DreamManager;
using Enum.Buff;
using Struct;
using UnityEngine;

namespace DreamSystem.Damage.Buff.Logic
{
    /// <summary>
    /// 事件触发型 Buff 逻辑。
    /// <para>在 OnApply 时订阅指定事件；事件触发且满足冷却条件时，向自身 BuffSystem 施加一个二级临时 Buff。</para>
    /// <para>stringParam 格式：<c>EVENT_NAME|secondaryBuffId|cooldownSeconds</c></para>
    /// <para>支持的事件：DAMAGE_RESULT（玩家受击）、PLAYER_DODGE_PERFORMED（闪避）、ENEMY_DEATH（击杀）。</para>
    /// </summary>
    [BuffLogic(BuffLogicType.TriggerOnEvent)]
    public class TriggerOnEventBuffLogic : BuffBaseLogic
    {
        private string _eventName;
        private string _secondaryBuffId;
        private float _cooldownSeconds;
        private float _lastTriggerTime = -999f;

        private EventManager EventMgr => buffInstance?.buffSystem?.EventManager;

        public override void OnApply()
        {
            var parts = (buffInstance?.data?.stringParam ?? "").Split('|');
            _eventName        = parts.Length > 0 ? parts[0].Trim() : "";
            _secondaryBuffId  = parts.Length > 1 ? parts[1].Trim() : "";
            float.TryParse(parts.Length > 2 ? parts[2].Trim() : "1", out _cooldownSeconds);

            if (string.IsNullOrEmpty(_eventName) || string.IsNullOrEmpty(_secondaryBuffId))
            {
                UnityEngine.Debug.LogWarning($"[TriggerOnEventBuff] stringParam 格式不完整: '{buffInstance?.data?.stringParam}'");
                return;
            }

            Subscribe();
        }

        public override void OnRemove()
        {
            Unsubscribe();
        }

        // ─────────────────────────────────────────────────────────────────
        // 订阅 / 取消订阅
        // ─────────────────────────────────────────────────────────────────

        private void Subscribe()
        {
            var em = EventMgr;
            if (em == null) return;

            switch (_eventName)
            {
                case GameEvents.DAMAGE_RESULT:
                    em.Subscribe<DamageResult>(GameEvents.DAMAGE_RESULT, OnDamageResult);
                    break;
                case GameEvents.PLAYER_DODGE_PERFORMED:
                    em.Subscribe<bool>(GameEvents.PLAYER_DODGE_PERFORMED, OnDodgePerformed);
                    break;
                case GameEvents.ENEMY_DEATH:
                    em.Subscribe<GameObject>(GameEvents.ENEMY_DEATH, OnEnemyDeath);
                    break;
                default:
                    UnityEngine.Debug.LogWarning($"[TriggerOnEventBuff] 未知事件名: '{_eventName}'");
                    break;
            }
        }

        private void Unsubscribe()
        {
            var em = EventMgr;
            if (em == null) return;

            switch (_eventName)
            {
                case GameEvents.DAMAGE_RESULT:
                    em.Unsubscribe<DamageResult>(GameEvents.DAMAGE_RESULT, OnDamageResult);
                    break;
                case GameEvents.PLAYER_DODGE_PERFORMED:
                    em.Unsubscribe<bool>(GameEvents.PLAYER_DODGE_PERFORMED, OnDodgePerformed);
                    break;
                case GameEvents.ENEMY_DEATH:
                    em.Unsubscribe<GameObject>(GameEvents.ENEMY_DEATH, OnEnemyDeath);
                    break;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 事件回调
        // ─────────────────────────────────────────────────────────────────

        private void OnDamageResult(DamageResult result)
        {
            // 只有当本 Buff 的 owner 受击时才触发
            if (owner?.Stats == null || result.TargetStats != owner.Stats) return;
            TryTrigger();
        }

        private void OnDodgePerformed(bool _) => TryTrigger();

        private void OnEnemyDeath(GameObject _) => TryTrigger();

        // ─────────────────────────────────────────────────────────────────
        // 触发逻辑（含冷却判断）
        // ─────────────────────────────────────────────────────────────────

        private void TryTrigger()
        {
            if (Time.time - _lastTriggerTime < _cooldownSeconds) return;
            _lastTriggerTime = Time.time;

            buffInstance?.buffSystem?.AddBuff(_secondaryBuffId);
            UnityEngine.Debug.Log($"[TriggerOnEventBuff] 触发 '{_secondaryBuffId}' via '{_eventName}'");
        }
    }
}

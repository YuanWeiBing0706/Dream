using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DreamSystem.UI.ViewModel;
using Enum.Buff;

namespace DreamSystem.UI.View
{
    /// <summary>
    /// 单条 StatType → 图标预制体的映射，在 Inspector 里配置 5 条（Health/Attack/Defense/Speed 等）。
    /// 预制体直接拖入，运行时通过 Instantiate 生成到 BuffIconContainer 下。
    /// </summary>
    [Serializable]
    public struct StatIconEntry
    {
        public StatType StatType;
        /// 图标预制体（例如 Icon_Stat_Attack）
        public GameObject Prefab;
    }

    public sealed class PlayerStatusView : UIView<PlayerStatusViewModel>
    {
        [Header("Top Left - Health")]
        [SerializeField] private GameObject HealthFill;

        [Header("Top Center - Wave Countdown")]
        [SerializeField] private GameObject WaveCountdownRoot;
        [SerializeField] private TMP_Text CountdownText;

        [Header("Top Right - Level & Resources")]
        [SerializeField] private TMP_Text LevelInfoText;
        [SerializeField] private TMP_Text GoldText;

        [Header("Bottom Center - Buffs")]
        [SerializeField] private Transform BuffIconContainer;
        [SerializeField] private GameObject BuffIconPrefab;

        [Header("Stat Icon Mapping (Inspector 配置 5 条，对应 5 种属性)")]
        [SerializeField] private StatIconEntry[] StatIconMappings;

        private Slider _healthSlider;
        private Transform _buffLayoutContainer;

        /// <summary>key = StatType，value = 当前显示中的图标 GameObject</summary>
        private readonly Dictionary<StatType, GameObject> _activeStatIcons = new();

        protected override void OnBindTyped(PlayerStatusViewModel viewModel)
        {
            if (HealthFill != null)
                _healthSlider = HealthFill.GetComponentInChildren<Slider>();

            // BuffIconPrefab 在 Inspector 里绑定的是 Buff_List_Container（带 LayoutGroup）
            _buffLayoutContainer = BuffIconPrefab != null ? BuffIconPrefab.transform : BuffIconContainer;

            RefreshUI();
        }

        protected override void OnViewModelRefreshRequested(int refreshKey)
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (Vm == null) return;

            if (_healthSlider != null)
                _healthSlider.value = Vm.CurrentHpFill;

            if (WaveCountdownRoot != null)
            {
                WaveCountdownRoot.SetActive(Vm.ShowCountdown);
                if (CountdownText != null)
                {
                    CountdownText.text = Vm.CountdownValue > 0
                        ? $"距离下一层\n还有{Vm.CountdownValue}秒"
                        : "进入下一关...";
                }
            }
            
            if (LevelInfoText != null)
                LevelInfoText.text = Vm.LevelInfo;

            if (GoldText != null)
                GoldText.text = $"{Vm.GoldCount}";

            RefreshStatIcons();
        }

        /// <summary>
        /// 根据 ViewModel.ModifiedStats 同步显示/隐藏 StatType 图标。
        /// - 有修改器生效（FinalValue ≠ BaseValue）→ 确保对应图标存在
        /// - 无修改器生效 → 销毁对应图标
        /// </summary>
        private void RefreshStatIcons()
        {
            if (_buffLayoutContainer == null) return;

            // 收集当前应显示的集合
            var shouldShow = new HashSet<StatType>(Vm.ModifiedStats);

            // 移除不再需要显示的图标
            var toRemove = new List<StatType>();
            foreach (var kv in _activeStatIcons)
            {
                if (!shouldShow.Contains(kv.Key))
                {
                    if (kv.Value != null) Destroy(kv.Value);
                    toRemove.Add(kv.Key);
                }
            }
            foreach (var key in toRemove)
                _activeStatIcons.Remove(key);

            // 添加尚未显示的图标
            foreach (var statType in shouldShow)
            {
                if (_activeStatIcons.ContainsKey(statType)) continue;

                var prefab = GetIconPrefab(statType);
                if (prefab == null)
                {
                    UnityEngine.Debug.LogWarning($"[PlayerStatusView] StatType={statType} 未配置图标预制体，跳过");
                    continue;
                }

                var iconGo = Instantiate(prefab, _buffLayoutContainer, false);
                iconGo.name = $"StatIcon_{statType}";
                _activeStatIcons[statType] = iconGo;
            }
        }

        private GameObject GetIconPrefab(StatType statType)
        {
            if (StatIconMappings == null) return null;
            foreach (var entry in StatIconMappings)
            {
                if (entry.StatType == statType)
                    return entry.Prefab;
            }
            return null;
        }
    }
}

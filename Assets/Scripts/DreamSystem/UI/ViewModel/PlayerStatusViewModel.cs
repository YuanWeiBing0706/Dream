using System;
using System.Collections.Generic;
using Data;
using DreamConfig;
using DreamManager;
using DreamSystem.Damage.Stat;
using Enum.Buff;
using Model.Player;
using VContainer;

namespace DreamSystem.UI.ViewModel
{
    public sealed class PlayerStatusViewModel : ViewModelBase
    {
        private readonly PlayerModel _playerModel;
        private readonly GameSessionData _sessionData;
        private readonly ResourcesManager _resources;

        public ResourcesManager Resources => _resources;

        public float CurrentHpFill
        {
            get
            {
                if (_playerModel?.Stats == null) return 1f;
                float max = GetFinalStatValue(StatType.Health);
                return max > 0 ? GetCurrentStatValue(StatType.Health) / max : 1f;
            }
        }

        public int GoldCount => _sessionData.CurrentCoinCount;
        public string LevelInfo => $"第{_sessionData.Chapter}关 第{_sessionData.Level}层";

        private int _countdownValue;
        public int CountdownValue => _countdownValue;

        private bool _showCountdown;
        public bool ShowCountdown => _showCountdown;

        /// <summary>
        /// 当前有修改器生效（FinalValue ≠ BaseValue）的属性类型列表。
        /// View 据此决定哪些 StatType 图标可见，生命周期与属性状态完全同步。
        /// </summary>
        private readonly List<StatType> _modifiedStats = new();
        public IReadOnlyList<StatType> ModifiedStats => _modifiedStats;

        [Inject]
        public PlayerStatusViewModel(PlayerModel playerModel, GameSessionData sessionData, ResourcesManager resources)
        {
            _playerModel = playerModel;
            _sessionData = sessionData;
            _resources = resources;
        }

        public override void OnEnter()
        {
            if (_playerModel == null) return;

            if (_playerModel.Stats != null)
                _playerModel.Stats.OnDataChanged += HandleStatsChanged;

            _playerModel.OnStatsInitialized += OnPlayerStatsReady;

            RebuildModifiedStats();
            NotifyRefresh();
        }

        public override void OnExit()
        {
            if (_playerModel != null)
            {
                if (_playerModel.Stats != null)
                    _playerModel.Stats.OnDataChanged -= HandleStatsChanged;
                _playerModel.OnStatsInitialized -= OnPlayerStatsReady;
            }
        }

        private void OnPlayerStatsReady()
        {
            if (_playerModel?.Stats != null)
                _playerModel.Stats.OnDataChanged += HandleStatsChanged;

            RebuildModifiedStats();
            NotifyRefresh();
        }

        private void HandleStatsChanged(int refreshKey)
        {
            RebuildModifiedStats();
            NotifyRefresh(refreshKey);
        }

        /// <summary>
        /// 扫描所有 StatType，将 FinalValue ≠ BaseValue 的属性加入可见列表。
        /// 该逻辑替代了原先基于 HexConfig.iconPath 的字符串路径方案：
        /// 无需追踪 Buff 生命周期，图标显示状态与属性实际状态完全同步。
        /// </summary>
        private void RebuildModifiedStats()
        {
            _modifiedStats.Clear();
            if (_playerModel?.Stats == null) return;

            foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
            {
                BaseStat stat = _playerModel.Stats.GetStat(statType);
                if (stat != null && Math.Abs(stat.FinalValue - stat.BaseValue) > 0.001f)
                    _modifiedStats.Add(statType);
            }
        }

        public void SetCountdown(int value, bool visible)
        {
            _countdownValue = value;
            _showCountdown = visible;
            NotifyRefresh();
        }

        /// <summary>
        /// 关卡推进后刷新层数/金币等会话数据显示（由 LevelManager.AdvanceProgression 调用）。
        /// </summary>
        public void RefreshSessionInfo()
        {
            NotifyRefresh();
        }

        public float GetFinalStatValue(StatType statType)
        {
            if (_playerModel?.Stats == null) return 100f;
            BaseStat stat = _playerModel.Stats.GetStat(statType);
            return (stat != null && stat.FinalValue > 0) ? stat.FinalValue : 100f;
        }

        public float GetCurrentStatValue(StatType statType)
        {
            if (_playerModel?.Stats == null) return 0f;
            return _playerModel.Stats.GetCurrentStatValue(statType);
        }
    }
}

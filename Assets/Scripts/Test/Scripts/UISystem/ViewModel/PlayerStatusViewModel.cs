// using StatSystem;
//
// public sealed class PlayerStatusViewModel : ViewModelBase
// {
//     private readonly CharacterStats _characterStats;
//
//     public PlayerStatusViewModel(CharacterStats characterStats)
//     {
//         _characterStats = characterStats;
//     }
//
//     public override void OnEnter()
//     {
//         if (_characterStats == null)
//         {
//             return;
//         }
//
//         _characterStats.onDataChanged += HandleDataChanged;
//         NotifyRefresh();
//     }
//
//     public override void OnExit()
//     {
//         if (_characterStats == null)
//         {
//             return;
//         }
//
//         _characterStats.onDataChanged -= HandleDataChanged;
//     }
//
//     private void HandleDataChanged(int refreshKey)
//     {
//         NotifyRefresh(refreshKey);
//     }
//
//     public float GetFinalStatValue(StatType statType)
//     {
//         if (_characterStats == null)
//         {
//             return 0f;
//         }
//
//         BaseStat stat = _characterStats.GetStat(statType);
//         return stat != null ? stat.FinalValue : 0f;
//     }
//
//     public float GetCurrentStatValue(StatType statType)
//     {
//         if (_characterStats == null)
//         {
//             return 0f;
//         }
//
//         return _characterStats.GetCurrentStatValue(statType);
//     }
// }
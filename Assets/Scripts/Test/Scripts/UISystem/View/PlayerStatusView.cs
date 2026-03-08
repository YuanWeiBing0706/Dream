// using TMPro;
// using StatSystem;
// using UnityEngine;
//
// public sealed class PlayerStatusView : UIView<PlayerStatusViewModel>
// {
//     [SerializeField] private TMP_Text attackText;
//     [SerializeField] private TMP_Text hpText;
//
//     protected override void OnBindTyped(PlayerStatusViewModel viewModel)
//     {
//         RefreshUI();
//     }
//
//     protected override void OnViewModelRefreshRequested(int refreshKey)
//     {
//         if (refreshKey == (int)StatType.Attack)
//         {
//             RefreshAttack();
//             return;
//         }
//
//         if (refreshKey == (int)StatType.Health)
//         {
//             RefreshHp();
//             return;
//         }
//
//         RefreshUI();
//     }
//
//     private void RefreshUI()
//     {
//         RefreshAttack();
//         RefreshHp();
//     }
//
//     private void RefreshAttack()
//     {
//         if (VM == null)
//         {
//             if (attackText != null)
//             {
//                 attackText.text = "ATK: 0";
//             }
//
//             return;
//         }
//
//         if (attackText != null)
//         {
//             attackText.text = "ATK: " + VM.GetFinalStatValue(StatType.Attack).ToString("0.##");
//         }
//     }
//
//     private void RefreshHp()
//     {
//         if (VM == null)
//         {
//             if (hpText != null)
//             {
//                 hpText.text = "HP: 0 / 0";
//             }
//
//             return;
//         }
//
//         if (hpText != null)
//         {
//             float currentHp = VM.GetCurrentStatValue(StatType.Health);
//             float maxHp = VM.GetFinalStatValue(StatType.Health);
//             hpText.text = "HP: " + currentHp.ToString("0.##") + " / " + maxHp.ToString("0.##");
//         }
//     }
// }

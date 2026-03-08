// using StatSystem;
// using UnityEngine;
//
// public class PlayerStatusUIBootstrap : MonoBehaviour
// {
//     [SerializeField] private UIManager uiManager;
//     [SerializeField] private CharacterHub characterHub;
//     [SerializeField] private string playerStatusPanelId = UIPanelIds.PlayerStatusView;
//
//     private void Start()
//     {
//         if (uiManager == null)
//         {
//             Debug.LogError("UIManager is not assigned.");
//             return;
//         }
//
//         if (characterHub == null)
//         {
//             characterHub = GetComponentInParent<CharacterHub>();
//         }
//
//         if (characterHub == null)
//         {
//             Debug.LogError("CharacterHub is not assigned.");
//             return;
//         }
//
//         if (!characterHub.TryGet(out CharacterStats characterStats))
//         {
//             Debug.LogError("CharacterStats is not initialized on CharacterHub.");
//             return;
//         }
//
//         uiManager.ShowView(playerStatusPanelId, new PlayerStatusViewModel(characterStats));
//     }
// }

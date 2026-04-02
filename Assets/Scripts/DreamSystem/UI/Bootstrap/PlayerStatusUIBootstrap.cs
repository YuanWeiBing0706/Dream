using DreamManager;
using UnityEngine;
namespace DreamSystem.UI.Bootstrap
{
    public class PlayerStatusUIBootstrap : MonoBehaviour
    {
        private UIManager uiManager;
        // /// 代表玩家核心数据的 CharacterHub 组件
        // [SerializeField] private CharacterHub characterHub;
        // /// 所绑定的玩家状态 UI 面板 ID
        // [SerializeField] private string playerStatusPanelId = UIPanelIds.PlayerStatusView;

        // /// <summary>
        // /// 启动时获取所需组件并在 UI 系统中注册显示玩家状态面板
        // /// </summary>
        // private void Start()
        // {
        //     if (uiManager == null)
        //     {
        //         // 缺少 UIManager，停止初始化
        //         Debug.LogError("UIManager is not assigned.");
        //         return;
        //     }
        //
        //     if (characterHub == null)
        //     {
        //         // 尝试从父对象上获取 CharacterHub
        //         characterHub = GetComponentInParent<CharacterHub>();
        //     }
        //
        //     if (characterHub == null)
        //     {
        //         // 如果依旧无法获取，则报出错误
        //         Debug.LogError("CharacterHub is not assigned.");
        //         return;
        //     }
        //
        //     if (!characterHub.TryGet(out CharacterStats characterStats))
        //     {
        //         // 从 Hub 中获取底层属性数据失败
        //         Debug.LogError("CharacterStats is not initialized on CharacterHub.");
        //         return;
        //     }
        //
        //     // 组合数据模型并显示相应的视图
        //     uiManager.ShowView(playerStatusPanelId, new PlayerStatusViewModel(characterStats));
        // }
    }
}

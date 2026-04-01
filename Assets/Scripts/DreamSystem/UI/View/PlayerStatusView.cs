// using TMPro;
// using StatSystem;
// using UnityEngine;
//
// public sealed class PlayerStatusView : UIView<PlayerStatusViewModel>
// {
//     /// 攻击力显示文本引用
//     [SerializeField] private TMP_Text attackText;
//     /// 血量显示文本引用
//     [SerializeField] private TMP_Text hpText;
//
//     /// <summary>
//     /// 当强类型模型初始化绑定成功时立刻执行一次全界面强制刷新
//     /// </summary>
//     /// <param name="viewModel">数据模型实例</param>
//     protected override void OnBindTyped(PlayerStatusViewModel viewModel)
//     {
//         RefreshUI();
//     }
//
//     /// <summary>
//     /// 接收来自模型的刷新事件并进行分类过滤处理
//     /// </summary>
//     /// <param name="refreshKey">变动属性的特定系统标识位</param>
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
//     /// <summary>
//     /// 全量刷新所有文本信息
//     /// </summary>
//     private void RefreshUI()
//     {
//         RefreshAttack();
//         RefreshHp();
//     }
//
//     /// <summary>
//     /// 拉取当前攻击力属性重绘局部对应的 UI 文本
//     /// </summary>
//     private void RefreshAttack()
//     {
//         if (VM == null)
//         {
//             if (attackText != null)
//             {
//                 // 模型为空时显示默认值
//                 attackText.text = "ATK: 0";
//             }
//
//             return;
//         }
//
//         if (attackText != null)
//         {
//             // 从强类型模型实例拉取数据并赋值到视图文本
//             attackText.text = "ATK: " + VM.GetFinalStatValue(StatType.Attack).ToString("0.##");
//         }
//     }
//
//     /// <summary>
//     /// 拉取当前和上限最新血量状态并重绘相应的 UI 文本
//     /// </summary>
//     private void RefreshHp()
//     {
//         if (VM == null)
//         {
//             if (hpText != null)
//             {
//                 // 模型不可用时兜底
//                 hpText.text = "HP: 0 / 0";
//             }
//
//             return;
//         }
//
//         if (hpText != null)
//         {
//             // 组合获取的格式化数据作为血量字符串
//             float currentHp = VM.GetCurrentStatValue(StatType.Health);
//             float maxHp = VM.GetFinalStatValue(StatType.Health);
//             hpText.text = "HP: " + currentHp.ToString("0.##") + " / " + maxHp.ToString("0.##");
//         }
//     }
// }

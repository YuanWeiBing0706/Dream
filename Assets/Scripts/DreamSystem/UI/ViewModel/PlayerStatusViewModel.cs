// using StatSystem;
//
// public sealed class PlayerStatusViewModel : ViewModelBase
// {
//     /// 玩家核心属性状态引用
//     private readonly CharacterStats _characterStats;
//
//     /// <summary>
//     /// 初始化状态拦截模型，注册底层状态依赖
//     /// </summary>
//     /// <param name="characterStats">包含最新数据和事件的属性对象</param>
//     public PlayerStatusViewModel(CharacterStats characterStats)
//     {
//         _characterStats = characterStats;
//     }
//
//     /// <summary>
//     /// ViewModel 开始工作的切入点，启动监听和初次广播
//     /// </summary>
//     public override void OnEnter()
//     {
//         if (_characterStats == null)
//         {
//             return;
//         }
//
//         // 绑定角色属性变更事件，以随时触发刷新
//         _characterStats.onDataChanged += HandleDataChanged;
//         NotifyRefresh();
//     }
//
//     /// <summary>
//     /// 结束监听时收回事件注册
//     /// </summary>
//     public override void OnExit()
//     {
//         if (_characterStats == null)
//         {
//             return;
//         }
//
//         // 离开时注销事件防止闭包泄漏
//         _characterStats.onDataChanged -= HandleDataChanged;
//     }
//
//     /// <summary>
//     /// 当属性层触发变化时，接管数据并向视图层转发刷新信标
//     /// </summary>
//     /// <param name="refreshKey">变动涉及的属性键标识</param>
//     private void HandleDataChanged(int refreshKey)
//     {
//         NotifyRefresh(refreshKey);
//     }
//
//     /// <summary>
//     /// 给视图层提供获取该属性包含所有加成后的最终数值的接口
//     /// </summary>
//     /// <param name="statType">期望的属性类别</param>
//     /// <returns>最终浮点属性数值</returns>
//     public float GetFinalStatValue(StatType statType)
//     {
//         if (_characterStats == null)
//         {
//             return 0f;
//         }
//
//         // 查找对应状态类型数据
//         BaseStat stat = _characterStats.GetStat(statType);
//         return stat != null ? stat.FinalValue : 0f;
//     }
//
//     /// <summary>
//     /// 给视图层提供获取该属性当前变动数值（例如血量或蓝量）的接口
//     /// </summary>
//     /// <param name="statType">期望的属性类别</param>
//     /// <returns>当前的浮点数值</returns>
//     public float GetCurrentStatValue(StatType statType)
//     {
//         if (_characterStats == null)
//         {
//             return 0f;
//         }
//
//         // 返回当前实时的值
//         return _characterStats.GetCurrentStatValue(statType);
//     }
// }
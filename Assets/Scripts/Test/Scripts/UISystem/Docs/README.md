# UISystem (MVVM)

## 架构目标
- 支持全屏 `View`：同一时刻只有一个激活界面。
- 支持弹窗 `Window`：可多开，按栈管理，可关闭顶层窗口。
- 使用 MVVM：`View` 监听 `ViewModel` 属性变化刷新 UI。

## 关键文件
- `UIManager.cs`：UI 统一入口，负责显示/关闭 `View` 和 `Window`。
- `UIPanelConfigSO.cs`：`ScriptableObject` 配置，保存 `panelId + kind + prefab`。
- `UIViewBase.cs`：UI 面板基类，处理 ViewModel 生命周期绑定。
- `ViewModelBase.cs`：提供 `SetProperty` 与属性通知。

## ScriptableObject 配置方式
1. 在 Project 视图中右键：`Create/UISystem/UI Panel Config`。
2. 创建多个配置资产（例如 `UI_PanelConfig_PlayerStatusView`、`UI_PanelConfig_BagWindow`）。
3. 每个配置里填写：
   - `panelId`：唯一 ID。
   - `kind`：`View` 或 `Window`。
   - `prefab`：`GameObject` 预制体（必须挂载 `UIViewBase` 派生脚本）。
4. 将这些配置资产拖到 `UIManager.panelConfigs` 列表。

## 玩家状态 UI 示例（攻击力/当前生命/生命上限）

### 示例脚本
- `PlayerStatusViewModel.cs`：从 `CharacterStats` 读取并监听数据变化。
- `PlayerStatusView.cs`：显示文本（攻击力、当前生命、生命上限）。
- `PlayerStatusUIBootstrap.cs`：启动时打开该全屏 View。
- `UIPanelIds.cs`：示例面板 ID 常量。

### 接入步骤
1. 创建一个 UI 预制体并挂载 `PlayerStatusView`。
2. 预制体上把两个 `TMP_Text` 赋值给：
   - `attackText`
   - `hpText`
3. 创建 `UIPanelConfigSO` 资产：
   - `panelId = PlayerStatusView`
   - `kind = View`
   - `prefab = 你的 PlayerStatusView 预制体`
4. 把该资产加入 `UIManager.panelConfigs`。
5. 场景里挂载 `PlayerStatusUIBootstrap`，并赋值：
   - `uiManager`
   - `characterHub`（由 Hub 提供 `CharacterStats`）
6. 运行后，攻击力、当前生命、生命上限会随着 `CharacterStats` 事件自动刷新。

## 代码调用示例
```csharp
uiManager.ShowView(UIPanelIds.PlayerStatusView, new PlayerStatusViewModel(characterStats));
uiManager.ShowWindow("BagWindow", new EmptyViewModel());
uiManager.CloseTopWindow();
```

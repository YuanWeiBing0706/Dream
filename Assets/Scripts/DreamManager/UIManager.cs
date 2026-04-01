using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DreamConfig;
using Enum.UI;
using Test.Scripts.UISystem.View;
using Test.Scripts.UISystem.ViewModel;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace DreamManager
{
    public class UIManager
    {
        private readonly ResourcesManager _resourcesManager;
        private readonly IObjectResolver _resolver;
        private readonly UIConfig _uiConfig;
        
        private readonly Dictionary<string, UIViewBase> _uiCachePool = new Dictionary<string, UIViewBase>();
        
        /// 记录正在处于“异步加载中”的面板令牌
        private readonly Dictionary<string, CancellationTokenSource> _loadingTokens = new Dictionary<string, CancellationTokenSource>();

        /// 当前正在展示的全屏视图界面
        private UIViewBase _currentView;
        /// 已打开的弹窗界面字典，Key: panelId
        private readonly Dictionary<string, UIViewBase> _openedWindows = new Dictionary<string, UIViewBase>();
        /// 已打开的弹窗界面栈，用于管理层级与关闭逻辑
        private readonly List<UIViewBase> _windowStack = new List<UIViewBase>();

        /// 全屏视图界面的挂载根节点
        private Transform _viewRoot;
        /// 弹窗界面的挂载根节点
        private Transform _windowRoot;
        /// 弹窗索引起始值
        private readonly int _windowOrderStart = 100;
        /// 下一个弹窗的层级索引
        private int _nextWindowOrder;

        /// <summary>
        /// UIManager 构造函数，由 VContainer 注入依赖
        /// </summary>
        /// <param name="resMgr">资源管理器</param>
        /// <param name="resolver">对象解析器</param>
        [Inject]
        public UIManager(ResourcesManager resMgr, IObjectResolver resolver)
        {
            _resourcesManager = resMgr;
            _resolver = resolver;
            _nextWindowOrder = _windowOrderStart;

            // 获取刚建好的纯数据 UIConfig
            _uiConfig = _resourcesManager.GetConfig<UIConfig>();
            
            // 初始化画布层级
            InitializeUIRoot();
        }

        /// <summary>
        /// 确保场景中有一个供挂载 UI 的全局 Canvas 根节点
        /// </summary>
        private void InitializeUIRoot()
        {
            GameObject rootObj = GameObject.Find("UIRoot");
            if (rootObj == null)
            {
                rootObj = new GameObject("UIRoot");
                var canvas = rootObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                // 注意：由于脱离了 MonoBehaviour，此处纯代码建布，如果在实战项目中建议用一个固定的预制体加载
                rootObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                rootObj.AddComponent<GraphicRaycaster>();
                GameObject.DontDestroyOnLoad(rootObj);
            }

            Transform vRoot = rootObj.transform.Find("ViewRoot");
            if (vRoot == null)
            {
                vRoot = new GameObject("ViewRoot").transform;
                vRoot.SetParent(rootObj.transform, false);
            }
            _viewRoot = vRoot;

            Transform wRoot = rootObj.transform.Find("WindowRoot");
            if (wRoot == null)
            {
                wRoot = new GameObject("WindowRoot").transform;
                wRoot.SetParent(rootObj.transform, false);
            }
            _windowRoot = wRoot;
        }

        /// <summary>
        /// 异步显示一个全屏视图界面，如果已有视图则先关闭旧视图
        /// </summary>
        /// <param name="panelId">视图的标识 ID</param>
        /// <param name="viewModel">绑定的视图模型，可选</param>
        /// <returns>返回实例化的视图组件，失败时返回 null</returns>
        public async UniTask<UIViewBase> ShowViewAsync(string panelId, IViewModel viewModel = null)
        {
            // 如果已经在开着的栈里了，或者正在加载中
            if (_currentView != null && _currentView.PanelId == panelId)
            {
                return _currentView;
            }

            if (_currentView != null)
            {
                // VContainer 下重构后，这里只需调用 UIView 的 Close 请求，其内部会委托回到 UIManager 进行 SetActive(false)
                _currentView.Close();
                _currentView = null;
            }

            UIViewBase view = await InternalLoadAndCreateUI(panelId, _viewRoot, viewModel);
            if (view != null)
            {
                _currentView = view;
            }

            return view;
        }

        /// <summary>
        /// 异步显示一个弹窗界面
        /// </summary>
        /// <param name="panelId">弹窗的标识 ID</param>
        /// <param name="viewModel">绑定的视图模型，可选</param>
        /// <returns>返回实例化的弹窗组件，失败时返回 null</returns>
        public async UniTask<UIViewBase> ShowWindowAsync(string panelId, IViewModel viewModel = null)
        {
            // 如果已存在展示中，先关掉（为了重新压栈更新层级到最前面）
            if (_openedWindows.ContainsKey(panelId))
            {
                CloseWindow(panelId);
            }

            UIViewBase window = await InternalLoadAndCreateUI(panelId, _windowRoot, viewModel);
            if (window == null)
            {
                return null;
            }

            _openedWindows[panelId] = window;
            _windowStack.Add(window);
            window.transform.SetAsLastSibling();
            ApplyWindowOrder(window);
            return window;
        }

        /// <summary>
        /// 内部统一加载分配核心入口，处理对象池与异步取消逻辑
        /// </summary>
        /// <param name="panelId">UI 面板标识 ID</param>
        /// <param name="parentRoot">面板实例需要挂载的父节点</param>
        /// <param name="viewModel">绑定的视图模型</param>
        /// <returns>加载成功返回 UIViewBase 实例，失败则返回 null</returns>
        private async UniTask<UIViewBase> InternalLoadAndCreateUI(string panelId, Transform parentRoot, IViewModel viewModel)
        {
            // 1. 尝试从常驻对象池“秒开”
            if (_uiCachePool.TryGetValue(panelId, out var cachedUI))
            {
                cachedUI.transform.SetParent(parentRoot, false);
                
                // 从配置中获取种类
                if (_uiConfig.TryGet(panelId, out var data))
                {
                    // 重新接驳 ViewModel 和生命周期红线
                    cachedUI.Initialize(panelId, data.uiKind, viewModel, OnPanelCloseRequested);
                    cachedUI.Open(); // 内部会执行 gameObject.SetActive(true)
                }
                return cachedUI;
            }

            // 2. 防连击切断旧加载
            if (_loadingTokens.TryGetValue(panelId, out var existingCts))
            {
                // 取消先前的加载
                existingCts.Cancel();
                existingCts.Dispose();
                _loadingTokens.Remove(panelId);
            }

            var cts = new CancellationTokenSource();
            _loadingTokens[panelId] = cts;

            try
            {
                // 3. 从只读配置中查寻址路径
                if (_uiConfig == null || !_uiConfig.TryGet(panelId, out var uiData))
                {
                    Debug.LogError($"[UIManager] 从 UIConfig 中找不到面板配置: {panelId}");
                    return null;
                }

                // 4. 使用异步加载，支持断网、按键撤销导致的截断
                var prefab = await _resourcesManager.LoadAssetAsync<GameObject>(uiData.assetPath, cts.Token);
                if (prefab == null) return null;

                // 5. 实例化并组装
                GameObject panelObject = GameObject.Instantiate(prefab, parentRoot);
                UIViewBase instance = panelObject.GetComponent<UIViewBase>();
                if (instance == null)
                {
                    Debug.LogError($"[UIManager] 预制体缺少 UIViewBase 脚本: {uiData.assetPath}");
                    GameObject.Destroy(panelObject);
                    return null;
                }
                
                instance.name = panelId;

                // 6. 放入常驻对象池！下一次就不用 await 加载了
                _uiCachePool[panelId] = instance;

                // 7. VContainer 装配 ViewModel 的手动绑定：
                instance.Initialize(panelId, uiData.uiKind, viewModel, OnPanelCloseRequested);
                instance.Open();

                return instance;
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[UIManager] 玩家在面板 {panelId} 加载中途点击了关闭，已安全熔断防重构连击!");
                return null;
            }
            finally
            {
                if (_loadingTokens.TryGetValue(panelId, out var currentToken) && currentToken == cts)
                {
                    _loadingTokens.Remove(panelId);
                    cts.Dispose();
                }
            }
        }

        /// <summary>
        /// 当面板触发关闭请求时的回调
        /// </summary>
        /// <param name="panel">请求关闭的面板实体</param>
        private void OnPanelCloseRequested(UIViewBase panel)
        {
            if (panel == null) return;
            if (panel.Kind == UIKind.View) { CloseCurrentView(); return; }
            CloseWindow(panel.PanelId);
        }

        /// <summary>
        /// 根据指定的 ID 关闭相应的弹窗界面
        /// </summary>
        /// <param name="panelId">待关闭的弹窗标识 ID</param>
        /// <returns>如果是合法窗口并成功隐藏则返回 true，否则返回 false</returns>
        public bool CloseWindow(string panelId)
        {
            // 查：是否属于“正在异步路上还没显示”出来的幽灵面版？
            if (_loadingTokens.TryGetValue(panelId, out var cts))
            {
                cts.Cancel(); 
                _loadingTokens.Remove(panelId);
                return true; 
            }

            if (!_openedWindows.TryGetValue(panelId, out UIViewBase window))
            {
                return false;
            }

            _openedWindows.Remove(panelId);
            _windowStack.Remove(window);

            // 绝不 Destroy！而是把游戏物体隐藏，送回对象池黑暗的角落保存
            window.gameObject.SetActive(false);
            
            return true;
        }

        /// <summary>
        /// 关闭显示在最上层的一层弹窗结构
        /// </summary>
        /// <returns>成功关闭则返回 true，若无弹窗可关闭则返回 false</returns>
        public bool CloseTopWindow()
        {
            if (_windowStack.Count <= 0) return false;
            UIViewBase top = _windowStack[_windowStack.Count - 1];
            return CloseWindow(top.PanelId);
        }

        /// <summary>
        /// 隐藏并关闭当前所有正在展示的全屏视图
        /// </summary>
        public void CloseCurrentView()
        {
            if (_currentView == null) return;
            // 隐藏入池
            _currentView.gameObject.SetActive(false);
            _currentView = null;
        }

        /// <summary>
        /// 清空位于上层的所有弹窗层级的内容
        /// </summary>
        public void CloseAllWindows()
        {
            for (int i = _windowStack.Count - 1; i >= 0; i--)
            {
                UIViewBase window = _windowStack[i];
                if (window != null)
                {
                    window.gameObject.SetActive(false);
                }
            }

            _openedWindows.Clear();
            _windowStack.Clear();
        }

        /// <summary>
        /// 获取当前全屏视图界面组件
        /// </summary>
        /// <typeparam name="T">期望转换的视图子类类型</typeparam>
        /// <returns>成功转换则返回视图组件，失败或空返回 null</returns>
        public T GetCurrentView<T>() where T : UIViewBase
        {
            return _currentView as T;
        }

        /// <summary>
        /// 根据指派的 ID 寻找其弹窗体组件本身
        /// </summary>
        /// <typeparam name="T">期望接收到的弹窗组件类型</typeparam>
        /// <param name="panelId">需要被查找的弹窗界面 ID</param>
        /// <returns>成功匹配则返回该实例，未命中或类型错误返回 null</returns>
        public T GetWindow<T>(string panelId) where T : UIViewBase
        {
            if (_openedWindows.TryGetValue(panelId, out UIViewBase window))
            {
                return window as T;
            }
            return null;
        }

        /// <summary>
        /// 赋予弹窗界面一个新的层级遮挡顺序
        /// </summary>
        /// <param name="panel">准备提升更新层级的弹窗对象</param>
        private void ApplyWindowOrder(UIViewBase panel)
        {
            Canvas panelCanvas = panel.GetComponent<Canvas>();
            if (panelCanvas == null) return;
            panelCanvas.overrideSorting = true;
            panelCanvas.sortingOrder = _nextWindowOrder;
            _nextWindowOrder++;
        }
    }
}

// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Serialization;
//
// public class UIManager : MonoBehaviour
// {
//     [Header("Root")]
//     [SerializeField] private Transform viewRoot;
//     [SerializeField] private Transform windowRoot;
//
//     [Header("Window Order")]
//     [SerializeField] private int windowOrderStart = 100;
//
//     [Header("Registry (ScriptableObject)")]
//     [SerializeField] private List<UIPanelConfigSO> panelConfigs = new List<UIPanelConfigSO>();
//
//     private readonly Dictionary<string, UIPanelConfigSO> _viewRegistry = new Dictionary<string, UIPanelConfigSO>();
//     private readonly Dictionary<string, UIPanelConfigSO> _windowRegistry = new Dictionary<string, UIPanelConfigSO>();
//     private readonly Dictionary<string, UIViewBase> _openedWindows = new Dictionary<string, UIViewBase>();
//     private readonly List<UIViewBase> _windowStack = new List<UIViewBase>();
//
//     private UIViewBase _currentView;
//     private int _nextWindowOrder;
//
//     private void Awake()
//     {
//         if (viewRoot == null)
//         {
//             viewRoot = transform;
//         }
//
//         if (windowRoot == null)
//         {
//             windowRoot = transform;
//         }
//
//         _nextWindowOrder = windowOrderStart;
//         BuildRegistry();
//     }
//
//     public UIViewBase ShowView(string panelId, IViewModel viewModel = null)
//     {
//         UIPanelConfigSO entry;
//         if (!_viewRegistry.TryGetValue(panelId, out entry))
//         {
//             Debug.LogError("View not registered: " + panelId);
//             return null;
//         }
//
//         if (_currentView != null)
//         {
//             _currentView.Close();
//             _currentView = null;
//         }
//
//         UIViewBase view = CreatePanel(entry, viewRoot, viewModel);
//         if (view == null)
//         {
//             return null;
//         }
//
//         _currentView = view;
//         return view;
//     }
//
//     public UIViewBase ShowWindow(string panelId, IViewModel viewModel = null)
//     {
//         UIPanelConfigSO entry;
//         if (!_windowRegistry.TryGetValue(panelId, out entry))
//         {
//             Debug.LogError("Window not registered: " + panelId);
//             return null;
//         }
//
//         if (_openedWindows.ContainsKey(panelId))
//         {
//             CloseWindow(panelId);
//         }
//
//         UIViewBase window = CreatePanel(entry, windowRoot, viewModel);
//         if (window == null)
//         {
//             return null;
//         }
//
//         _openedWindows[panelId] = window;
//         _windowStack.Add(window);
//         window.transform.SetAsLastSibling();
//         ApplyWindowOrder(window);
//         return window;
//     }
//
//     public bool CloseWindow(string panelId)
//     {
//         UIViewBase window;
//         if (!_openedWindows.TryGetValue(panelId, out window))
//         {
//             return false;
//         }
//
//         _openedWindows.Remove(panelId);
//         _windowStack.Remove(window);
//         window.Close();
//         return true;
//     }
//
//     public bool CloseTopWindow()
//     {
//         if (_windowStack.Count <= 0)
//         {
//             return false;
//         }
//
//         UIViewBase top = _windowStack[_windowStack.Count - 1];
//         return CloseWindow(top.PanelId);
//     }
//
//     public void CloseCurrentView()
//     {
//         if (_currentView == null)
//         {
//             return;
//         }
//
//         _currentView.Close();
//         _currentView = null;
//     }
//
//     public void CloseAllWindows()
//     {
//         for (int i = _windowStack.Count - 1; i >= 0; i--)
//         {
//             UIViewBase window = _windowStack[i];
//             if (window == null)
//             {
//                 continue;
//             }
//
//             window.Close();
//         }
//
//         _openedWindows.Clear();
//         _windowStack.Clear();
//     }
//
//     public T GetCurrentView<T>() where T : UIViewBase
//     {
//         return _currentView as T;
//     }
//
//     public T GetWindow<T>(string panelId) where T : UIViewBase
//     {
//         UIViewBase window;
//         if (!_openedWindows.TryGetValue(panelId, out window))
//         {
//             return null;
//         }
//
//         return window as T;
//     }
//
//     private UIViewBase CreatePanel(UIPanelConfigSO entry, Transform root, IViewModel viewModel)
//     {
//         GameObject panelObject = Instantiate(entry.prefab, root);
//         UIViewBase instance = panelObject.GetComponent<UIViewBase>();
//         if (instance == null)
//         {
//             Debug.LogError("Prefab does not contain UIViewBase: " + entry.panelId);
//             Destroy(panelObject);
//             return null;
//         }
//
//         instance.name = entry.panelId;
//         instance.Initialize(entry.panelId, entry.kind, viewModel ?? new EmptyViewModel(), OnPanelCloseRequested);
//         instance.Open();
//         return instance;
//     }
//
//     private void OnPanelCloseRequested(UIViewBase panel)
//     {
//         if (panel == null)
//         {
//             return;
//         }
//
//         if (panel.Kind == UIKind.View)
//         {
//             if (_currentView == panel)
//             {
//                 CloseCurrentView();
//             }
//
//             return;
//         }
//
//         CloseWindow(panel.PanelId);
//     }
//
//     private void ApplyWindowOrder(UIViewBase panel)
//     {
//         Canvas panelCanvas = panel.GetComponent<Canvas>();
//         if (panelCanvas == null)
//         {
//             return;
//         }
//
//         panelCanvas.overrideSorting = true;
//         panelCanvas.sortingOrder = _nextWindowOrder;
//         _nextWindowOrder++;
//     }
//
//     private void BuildRegistry()
//     {
//         _viewRegistry.Clear();
//         _windowRegistry.Clear();
//
//         for (int i = 0; i < panelConfigs.Count; i++)
//         {
//             UIPanelConfigSO entry = panelConfigs[i];
//             if (entry == null || string.IsNullOrEmpty(entry.panelId) || entry.prefab == null)
//             {
//                 continue;
//             }
//
//             if (entry.kind == UIKind.View)
//             {
//                 if (_viewRegistry.ContainsKey(entry.panelId))
//                 {
//                     Debug.LogWarning("Duplicate View panelId: " + entry.panelId);
//                 }
//
//                 _viewRegistry[entry.panelId] = entry;
//                 continue;
//             }
//
//             if (_windowRegistry.ContainsKey(entry.panelId))
//             {
//                 Debug.LogWarning("Duplicate Window panelId: " + entry.panelId);
//             }
//
//             _windowRegistry[entry.panelId] = entry;
//         }
//     }
// }

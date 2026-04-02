using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DreamAttribute;
using DreamManager;
using Enum.UI;
using Function;

namespace DreamConfig
{
    [Config]
    public class UIConfig : Config
    {
        private readonly Dictionary<string, UIData> _uiDataDir = new Dictionary<string, UIData>();
        private readonly List<UIData> _allUIList = new List<UIData>();

        public override UniTask LoadConfig(ResourcesManager resourcesManager)
        {
            var textAsset = resourcesManager.LoadAsset<UnityEngine.TextAsset>(nameof(UIConfig));
            var data = CsvHelper.ReadCsv(textAsset);

            for (int i = 0; i < data.Count; i++)
            {
                var uiData = new UIData
                {
                    panelId = data[i][0].Trim(),
                    assetPath = data[i][1].Trim(),
                    kind = System.Enum.Parse<UIKind>(data[i][2].Trim(), true),
                    baseSortOrder = int.Parse(data[i][3].Trim()),
                    isCacheable = bool.Parse(data[i][4].Trim())
                };
                
                if (!_uiDataDir.TryAdd(uiData.panelId, uiData)) 
                {
                    UnityEngine.Debug.LogError($"[UIConfig] 发现重复面板ID: {uiData.panelId}");
                    continue;
                }
                _allUIList.Add(uiData);
            }

            UnityEngine.Debug.Log($"[UIConfig] 成功加载 {_uiDataDir.Count} 条 UI 配置项目。");
            return UniTask.CompletedTask;
        }

        public UIData this[string panelId] => _uiDataDir[panelId];
        
        public bool TryGet(string panelId, out UIData data) => _uiDataDir.TryGetValue(panelId, out data);
        
        public List<UIData> GetAllUIList() => _allUIList;
    }

    public struct UIData
    {
        public string panelId;
        public string assetPath;
        public UIKind kind;
        public int baseSortOrder;
        public bool isCacheable;
    }
}
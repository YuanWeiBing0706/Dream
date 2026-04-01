using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DreamManager;
using Enum.UI;
using Function;
using UnityEngine;
namespace DreamConfig
{
    public class UIConfig : Config
    {
        private readonly Dictionary<string, UIData> _uiDataDir = new();
        private readonly List<UIData> _allUIList = new List<UIData>();
        
        public override UniTask LoadConfig(ResourcesManager resourcesManager)
        {
            var textAsset = resourcesManager.LoadAsset<TextAsset>(nameof(UIConfig));
            
            var data = CsvHelper.ReadCsv(textAsset);

            for (int i = 0; i < data.Count; i++)
            {
                var uiData = new UIData
                {
                    uiId = data[i][0],
                    assetPath = data[i][1],
                    uiKind = System.Enum.Parse<UIKind>(data[i][2], true),
                    baseSortOrder = int.Parse(data[i][3]),
                    isCacheable = bool.Parse(data[i][4]),
                };
                if (!_uiDataDir.TryAdd(uiData.uiId, uiData)) {
                    Debug.LogError($"[ItemConfig] 发现重复uiID: {uiData.uiId}");
                    continue;
                }
                _allUIList.Add(uiData);
            }

            return UniTask.CompletedTask;
        }
        
        public UIData this[string uiId]
        {
            get
            {
                return _uiDataDir[uiId];
            }
        }
        
        public bool TryGet(string uiId, out UIData data) => _uiDataDir.TryGetValue(uiId, out data);
        
        public List<UIData> GetAllItemList() => _allUIList;
    }
    
    
    public struct UIData 
    {
        public string uiId;
        /// 资源路径
        public string assetPath;
        /// 是否全屏幕
        public UIKind uiKind;
        /// 层级排序器
        public int baseSortOrder;
        /// 缓存开关
        public bool isCacheable;
    }
}
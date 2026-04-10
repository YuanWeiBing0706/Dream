using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DreamConfig;
using DreamManager;

namespace DreamSystem.UI.View
{
    /// <summary>
    /// 海克斯选择卡片：用于显示海克斯的图标、标题、描述并响应点击。
    /// </summary>
    public class HexCardItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text TitleText;
        [SerializeField] private TMP_Text DescriptionText;
        [SerializeField] private Image IconImage;
        [SerializeField] private Button SelectionButton;

        private System.Action<string> _onSelected;
        private string _hexId;

        /// <summary>
        /// 初始化卡片内容
        /// </summary>
        /// <param name="data">海克斯配置数据</param>
        /// <param name="resources">资源管理器（用于 YooAsset 会加载）</param>
        /// <param name="onSelect">选中回调</param>
        public void Setup(HexData data, ResourcesManager resources, System.Action<string> onSelect)
        {
            _hexId = data.hexId;
            _onSelected = onSelect;

            if (TitleText != null) TitleText.text = data.hexName;
            if (DescriptionText != null) DescriptionText.text = data.description;

            // 加载图标 (YooAsset)
            if (IconImage != null && !string.IsNullOrEmpty(data.iconPath))
            {
                var sprite = resources.LoadAsset<Sprite>(data.iconPath);
                if (sprite != null) IconImage.sprite = sprite;
            }

            // 按钮事件
            if (SelectionButton != null)
            {
                SelectionButton.onClick.RemoveAllListeners();
                SelectionButton.onClick.AddListener(OnClick);
            }
        }

        private void OnClick()
        {
            _onSelected?.Invoke(_hexId);
        }
    }
}

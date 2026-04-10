using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DreamSystem.UI.View
{
    /// <summary>
    /// 道具卡片（ItemCardNode 预制体的组件）。
    /// <para>
    /// 预制体结构：根节点挂 Image + Button，无子节点。<br/>
    /// - 点击切换选中状态，选中 = 原始颜色，未选中 = 亮度减半。<br/>
    /// - 鼠标悬浮时通知 ItemSelectView 显示 ItemInfo 悬浮卡。
    /// </para>
    /// </summary>
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class ItemCardNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Image _image;
        private Button _button;

        private int _index;
        private bool _isSelected;
        private string _itemName;
        private string _itemDesc;
        private Color _originalColor;

        private System.Action<int> _onToggle;
        private System.Action<string, string> _onHoverEnter;
        private System.Action _onHoverExit;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _button = GetComponent<Button>();
        }

        /// <summary>
        /// 初始化卡片。
        /// </summary>
        /// <param name="index">在 Rewards 列表中的索引</param>
        /// <param name="itemName">道具名称（传给 ItemInfo）</param>
        /// <param name="itemDesc">道具描述（传给 ItemInfo）</param>
        /// <param name="defaultSelected">初始是否选中</param>
        /// <param name="onToggle">选中状态改变回调（参数：列表索引）</param>
        /// <param name="onHoverEnter">鼠标进入回调（参数：名称、描述）</param>
        /// <param name="onHoverExit">鼠标离开回调</param>
        /// <param name="iconSprite">道具图标（可选）。不为 null 时会替换 Image 的 sprite。</param>
        public void Setup(
            int index,
            string itemName,
            string itemDesc,
            bool defaultSelected,
            System.Action<int> onToggle,
            System.Action<string, string> onHoverEnter,
            System.Action onHoverExit,
            Sprite iconSprite = null)
        {
            // 当父节点 inactive 时 Instantiate 不会触发 Awake，在此补做懒初始化
            if (_image == null) _image = GetComponent<Image>();
            if (_button == null) _button = GetComponent<Button>();

            _index = index;
            _itemName = itemName;
            _itemDesc = itemDesc;
            _isSelected = defaultSelected;
            _onToggle = onToggle;
            _onHoverEnter = onHoverEnter;
            _onHoverExit = onHoverExit;

            if (_image != null)
            {
                if (iconSprite != null)
                    _image.sprite = iconSprite;
                _originalColor = _image.color;
            }

            if (_button == null)
            {
                UnityEngine.Debug.LogWarning($"[ItemCardNode] {name} 上找不到 Button 组件，请检查预制体。");
                return;
            }

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClick);

            UpdateVisual();
        }

        private void OnClick()
        {
            _isSelected = !_isSelected;
            UpdateVisual();
            _onToggle?.Invoke(_index);
        }

        private void UpdateVisual()
        {
            if (_image == null) return;
            _image.color = _isSelected
                ? _originalColor
                : new Color(_originalColor.r * 0.45f, _originalColor.g * 0.45f, _originalColor.b * 0.45f, _originalColor.a);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _onHoverEnter?.Invoke(_itemName, _itemDesc);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _onHoverExit?.Invoke();
        }

        private void OnDisable()
        {
            // 对象被隐藏/销毁时确保 ItemInfo 关闭
            _onHoverExit?.Invoke();
        }
    }
}

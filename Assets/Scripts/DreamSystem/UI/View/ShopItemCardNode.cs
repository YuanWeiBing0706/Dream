using NodeCanvas.Tasks.Actions;
using Struct;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace DreamSystem.UI.View
{    /// <summary>
    /// 商店卡片（ItemShopInfo 预制体组件）。
    /// 结构要求：根节点挂 Button（可选再挂 Image），子节点 ItemIcon(Image)、ItemPrice(TMP)。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ShopItemCardNode: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image ItemIcon;
        [SerializeField] private TMP_Text ItemPrice;
        [Tooltip("用于选中/未选中染色的目标 Image（可不填，默认用 ItemIcon）")]
        [SerializeField] private Image TintTarget;

        private Button _button;
        private ShopOffer _offer;
        private bool _selected;
        private Color _baseColor;
        private string _hoverTitle;
        private string _hoverDesc;

        private System.Action<ShopOffer> _onClick;
        private System.Action<string, string> _onHoverEnter;
        private System.Action _onHoverExit;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        public void Setup(
            ShopOffer offer,
            bool selected,
            Sprite iconSprite,
            string hoverTitle,
            string hoverDesc,
            System.Action<ShopOffer> onClick,
            System.Action<string, string> onHoverEnter,
            System.Action onHoverExit)
        {
            if (_button == null) _button = GetComponent<Button>();
            if (ItemIcon == null) ItemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
            if (ItemPrice == null)
            {
                var t = transform.Find("ItemPrice");
                if (t != null)
                    ItemPrice = t.GetComponent<TMP_Text>() ?? t.GetComponentInChildren<TMP_Text>(true);
            }
            if (TintTarget == null) TintTarget = ItemIcon;

            _offer = offer;
            _selected = selected;
            _hoverTitle = hoverTitle;
            _hoverDesc = hoverDesc;
            _onClick = onClick;
            _onHoverEnter = onHoverEnter;
            _onHoverExit = onHoverExit;

            if (ItemIcon != null)
            {
                if (iconSprite != null) ItemIcon.sprite = iconSprite;
                _baseColor = ItemIcon.color;
            }

            if (ItemPrice != null)
                ItemPrice.text = $"{offer.Price}";

            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(() => _onClick?.Invoke(_offer));
            }

            UpdateVisual();
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            var target = TintTarget != null ? TintTarget : ItemIcon;
            if (target == null) return;

            target.color = _selected
                ? _baseColor
                : new Color(_baseColor.r * 0.55f, _baseColor.g * 0.55f, _baseColor.b * 0.55f, _baseColor.a);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _onHoverEnter?.Invoke(_hoverTitle, _hoverDesc);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _onHoverExit?.Invoke();
        }

        private void OnDisable()
        {
            _onHoverExit?.Invoke();
        }
    }
}
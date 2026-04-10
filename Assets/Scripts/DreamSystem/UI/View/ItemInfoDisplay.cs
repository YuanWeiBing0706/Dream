using TMPro;
using UnityEngine;

namespace DreamSystem.UI.View
{
    /// <summary>
    /// 道具信息悬浮卡（ItemInfo 预制体的组件）。
    /// <para>跟随鼠标位置，由 ItemSelectView 控制显示与隐藏。</para>
    /// </summary>
    public class ItemInfoDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text ItemName;
        [SerializeField] private TMP_Text ItemMessage;

        private RectTransform _rect;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            // 锚点固定左下角，方便用屏幕坐标直接定位
            _rect.anchorMin = Vector2.zero;
            _rect.anchorMax = Vector2.zero;
            _rect.pivot = new Vector2(0f, 1f); // 左上角对齐鼠标
            gameObject.SetActive(false);
        }

        /// <summary>显示道具信息并开始跟随鼠标。</summary>
        public void Show(string name, string desc)
        {
            if (ItemName != null) ItemName.text = name;
            if (ItemMessage != null) ItemMessage.text = desc;
            gameObject.SetActive(true);
        }

        /// <summary>隐藏悬浮卡。</summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            // Screen Space Overlay 下，RectTransform.position 对应屏幕像素坐标
            _rect.position = new Vector3(
                Input.mousePosition.x + 12f,
                Input.mousePosition.y - 12f,
                0f);
        }
    }
}

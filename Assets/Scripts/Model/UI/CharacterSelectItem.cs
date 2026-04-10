using System;
using UnityEngine;
using UnityEngine.UI;
namespace Model.UI
{
    /// <summary>
    /// 挂载在选角列表的头像项或确认按钮上
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class CharacterSelectItem : MonoBehaviour
    {
        [Header("英雄唯一ID")]
        public string CharacterId;

        [Header("选中反馈（可选）")]
        [SerializeField] private GameObject SelectedAffirmObject;
        [SerializeField] private Image CardImage;
        [SerializeField] private Color SelectedColor = Color.white;
        [SerializeField] private Color UnselectedColor = new Color(0.75f, 0.75f, 0.75f, 1f);

        private Button _button;
        private Action<string> _onClicked;
        private Vector3 _originScale;
        private Color _originColor = Color.white;
        private bool _hasOriginColor;

        /// <summary>
        /// 由父界面调用初始化
        /// </summary>
        public void Init(Action<string> onClickCallback)
        {
            _button = GetComponent<Button>();
            _onClicked = onClickCallback;

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => _onClicked?.Invoke(CharacterId));

            _originScale = transform.localScale;

            // 自动兜底：如果未手动拖拽，尝试按命名找到 *_Affirm 子节点
            if (SelectedAffirmObject == null)
            {
                var affirm = transform.Find($"{name}_Affirm");
                if (affirm != null) SelectedAffirmObject = affirm.gameObject;
            }
            if (CardImage == null)
                CardImage = GetComponent<Image>();
            if (CardImage != null)
            {
                _originColor = CardImage.color;
                _hasOriginColor = true;
            }

            SetSelected(false);
        }

        public void SetSelected(bool isSelected)
        {
            if (SelectedAffirmObject != null)
                SelectedAffirmObject.SetActive(isSelected);

            if (CardImage != null)
                CardImage.color = isSelected
                    ? SelectedColor
                    : (_hasOriginColor ? _originColor : UnselectedColor);

            transform.localScale = isSelected ? _originScale * 1.05f : _originScale;
        }
    }
}
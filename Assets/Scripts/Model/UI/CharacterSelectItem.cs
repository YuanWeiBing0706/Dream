using System;
using UnityEngine;
using UnityEngine.Serialization;
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
        
        private Button _button;
        private Action<string> _onClicked;

        /// <summary>
        /// 由父界面调用初始化
        /// </summary>
        public void Init(Action<string> onClickCallback)
        {
            _button = GetComponent<Button>();
            _onClicked = onClickCallback;
            
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => _onClicked?.Invoke(CharacterId));
        }

        // 可以在这里加一些被选中的高亮逻辑
        public void SetSelected(bool isSelected)
        {
            // 例如：修改边框颜色等
        }
    }
}
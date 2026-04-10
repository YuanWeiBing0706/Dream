using DreamSystem.UI.ViewModel;
using Model.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DreamSystem.UI.View
{
    public class CharacterSelectView : UIView<CharacterSelectViewModel>
    {
        [Header("交互按钮")]
        [SerializeField] private Button ConfirmBtn;
        [SerializeField] private Button CloseBtn;

        [Header("英雄列表")]
        [SerializeField] private CharacterSelectItem[] CharacterItems;

        /// 当前选中的英雄唯一ID
        private string _selectedHeroId;
        private CharacterSelectItem _selectedItem;

        protected override void OnBindTyped(CharacterSelectViewModel viewModel)
        {
            if (ConfirmBtn != null)
            {
                ConfirmBtn.onClick.AddListener(OnConfirmClicked);
            }

            if (CloseBtn != null)
            {
                CloseBtn.onClick.AddListener(OnCloseClicked);
            }

            // 初始化所有英雄项
            if (CharacterItems != null)
            {
                foreach (var item in CharacterItems)
                {
                    if (item == null) continue;
                    item.Init(OnCharacterSelected);
                }
            }

            // 每次打开面板时，恢复按钮可交互状态
            if (ConfirmBtn != null) ConfirmBtn.interactable = false; // 必须先选一个职业
            if (CloseBtn != null) CloseBtn.interactable = true;

            _selectedHeroId = null;
            _selectedItem = null;
            UpdateSelectionVisual();
        }

        protected override void OnUnbindTyped(CharacterSelectViewModel viewModel)
        {
            if (ConfirmBtn != null) ConfirmBtn.onClick.RemoveListener(OnConfirmClicked);
            if (CloseBtn != null) CloseBtn.onClick.RemoveListener(OnCloseClicked);
        }

        /// 当任何一个英雄项被点击时触发
        private void OnCharacterSelected(string heroId)
        {
            if (string.IsNullOrWhiteSpace(heroId))
            {
                UnityEngine.Debug.LogWarning("[CharacterSelect] 该卡片未配置 CharacterId。");
                return;
            }

            CharacterSelectItem clickedItem = null;
            if (CharacterItems != null)
            {
                foreach (var item in CharacterItems)
                {
                    if (item != null && item.CharacterId == heroId)
                    {
                        clickedItem = item;
                        break;
                    }
                }
            }

            // 先还原旧选中，再应用新选中，避免旧卡残留高亮
            if (_selectedItem != null && _selectedItem != clickedItem)
                _selectedItem.SetSelected(false);

            _selectedHeroId = heroId;
            _selectedItem = clickedItem;
            UnityEngine.Debug.Log($"[CharacterSelect] 玩家选择了: {heroId}");

            if (_selectedItem != null)
                _selectedItem.SetSelected(true);
            else
                // 若未找到对应项（例如 CharacterId 重复/未配），降级为全量刷新
                UpdateSelectionVisual();

            if (ConfirmBtn != null)
                ConfirmBtn.interactable = true;
        }

        /// 更新子项的视觉选中状态
        private void UpdateSelectionVisual()
        {
            if (CharacterItems == null)
            {
                return;
            }
            
            foreach (var item in CharacterItems)
            {
                if (item == null)
                {
                    continue;
                }
                
                item.SetSelected(item.CharacterId == _selectedHeroId);
            }
        }

        private void OnConfirmClicked()
        {
            if (string.IsNullOrEmpty(_selectedHeroId))
            {
                UnityEngine.Debug.LogWarning("[CharacterSelect] 请先选择一个英雄！");
                return;
            }

            // 防止连续多次点击导致加载多次场景
            ConfirmBtn.interactable = false;
            UnityEngine.Debug.Log($"当前选中的英雄为：{_selectedHeroId},开始进入游戏。");
            // 将真正选中的 ID 传给 VM 进入战斗
            this.Vm.ConfirmCharacterAndEnterGame(_selectedHeroId);
        }

        private void OnCloseClicked()
        {
            this.Vm.CancelSelection();
        }
    }
}

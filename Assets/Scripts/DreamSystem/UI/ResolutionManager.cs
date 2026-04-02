using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace DreamSystem.UI
{
    public class ResolutionManager : MonoBehaviour
    {
        [Header("UI References")] 
        /// 分辨率选择下拉框
        public TMP_Dropdown resolutionDropdown;
        /// 应用设置按钮
        public Button applyButton;
        /// 关闭界面按钮
        public Button closeButton;
        /// 包含整个设置界面的面板 GameObject
        public GameObject settingsPanel;
        /// 屏幕安全区域适配器
        [SerializeField] private MatchSafeArea _matchSafeArea;
        /// 当前设备可用的分辨率数组
        private Resolution[] availableResolutions;
        /// 下拉菜单所需显示的分辨率字符串列表
        private List<string> resolutionOptions = new List<string>();
        /// 当前选中的分辨率列表索引
        private int currentResolutionIndex = 0;
    
        /// <summary>
        /// 初始化分辨率列表并绑定 UI 事件
        /// </summary>
        void Start()
        {
            GetAvailableResolutions();
            PopulateDropdown();
            SetInitialResolution();

            // 绑定下拉菜单和按钮事件
            resolutionDropdown.onValueChanged.AddListener(SetResolutionIndex);
            applyButton.onClick.AddListener(ApplyResolution);
        }

        /// <summary>
        /// 读取系统支持的分辨率并进行去重和排序
        /// </summary>
        void GetAvailableResolutions()
        {
            // 获取所有刷新率去重并按像素与刷新率降序排列
            availableResolutions = Screen.resolutions.DistinctBy(res => new { res.width, res.height })
                .OrderByDescending(res => res.width * res.height) 
                .ThenByDescending(res => res.refreshRateRatio.value) 
                .ToArray();
        }

        /// <summary>
        /// 填充下拉菜单控件的选项内容
        /// </summary>
        void PopulateDropdown()
        {
            resolutionOptions.Clear();
            for (int i = 0; i < availableResolutions.Length; i++)
            {
                Resolution res = availableResolutions[i];
                string option = res.width + " x " + res.height;
                resolutionOptions.Add(option);
            }

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(resolutionOptions);
        }

        /// <summary>
        /// 匹配当前屏幕分辨率并将其设为下拉菜单的初始值
        /// </summary>
        void SetInitialResolution()
        {
            Resolution currentScreenResolution = Screen.currentResolution;

            for (int i = 0; i < availableResolutions.Length; i++)
            {
                // 通过宽高的精确匹配找到当前索引
                if (availableResolutions[i].width == currentScreenResolution.width &&
                    availableResolutions[i].height == currentScreenResolution.height)
                {
                    currentResolutionIndex = i;
                    break;
                }
            }

            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }

        /// <summary>
        /// 从 UI 回调接收新选择的分辨率索引
        /// </summary>
        /// <param name="index">下拉菜单对应选项索引</param>
        public void SetResolutionIndex(int index)
        {
            currentResolutionIndex = index;
        }

        /// <summary>
        /// 应用被选中的分辨率到游戏主屏幕窗口并刷新安全区域
        /// </summary>
        public void ApplyResolution()
        {
            if (currentResolutionIndex >= 0 && currentResolutionIndex < availableResolutions.Length)
            {
                Resolution selectedResolution = availableResolutions[currentResolutionIndex];
            
                // 设定游戏分辨率与全屏状态
                Screen.SetResolution(selectedResolution.width, selectedResolution.height, FullScreenMode.Windowed,
                    selectedResolution.refreshRateRatio);
            
                _matchSafeArea.OnResolutionChanged();
            }
        }

        /// <summary>
        /// 控制设置面板在显示与隐藏之间进行切换
        /// </summary>
        public void ToggleSettingsPanel()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf);
            }
        }
    }
}
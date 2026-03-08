using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionManager : MonoBehaviour
{
    [Header("UI References")] 
    public TMP_Dropdown resolutionDropdown;
    public Button applyButton;
    public Button closeButton;
    public GameObject settingsPanel; // 整个设置面板
    [SerializeField] private MatchSafeArea _matchSafeArea;
    private Resolution[] availableResolutions;
    private List<string> resolutionOptions = new List<string>();
    private int currentResolutionIndex = 0;
    
    void Start()
    {
      
        GetAvailableResolutions();
        PopulateDropdown();
        SetInitialResolution();

        // 订阅UI事件
        resolutionDropdown.onValueChanged.AddListener(SetResolutionIndex);
        applyButton.onClick.AddListener(ApplyResolution);
        // closeButton.onClick.AddListener(ToggleSettingsPanel); // Close按钮也用于打开/关闭面板

        // 如果想在游戏开始时强制应用上次保存的分辨率，可以在这里调用
        // ApplyResolution();
    }

    // 获取所有可用的分辨率并进行去重和排序
    void GetAvailableResolutions()
    {
        // Screen.resolutions 返回的是一个包含所有刷新率的列表
        // 我们需要去重并获取每个分辨率的最佳刷新率（或第一个）
        availableResolutions = Screen.resolutions.DistinctBy(res => new { res.width, res.height })
            .OrderByDescending(res => res.width * res.height) // 按分辨率大小降序
            .ThenByDescending(res => res.refreshRate) // 相同分辨率下按刷新率降序
            .ToArray();
    }

    // 填充下拉菜单
    void PopulateDropdown()
    {
        resolutionOptions.Clear();
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            Resolution res = availableResolutions[i];
            string option = res.width + " x " + res.height; // + " (" + res.refreshRate + "Hz)"; // 如果需要显示刷新率
            resolutionOptions.Add(option);
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(resolutionOptions);
    }

    // 设置下拉菜单的初始值，匹配当前屏幕分辨率
    void SetInitialResolution()
    {
        Resolution currentScreenResolution = Screen.currentResolution;
        // Debug.Log($"Current Screen Resolution: {currentScreenResolution.width}x{currentScreenResolution.height}@{currentScreenResolution.refreshRate}Hz");

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            if (availableResolutions[i].width == currentScreenResolution.width &&
                availableResolutions[i].height == currentScreenResolution.height)
                // 对于刷新率，由于不同平台获取方式可能不同，简单匹配宽高更稳妥
            {
                currentResolutionIndex = i;
                break;
            }
        }

        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue(); // 刷新显示值
        // Debug.Log($"Initial Dropdown Value Set to Index: {currentResolutionIndex}");
    }

    // 当下拉菜单值改变时调用，更新选中的分辨率索引
    public void SetResolutionIndex(int index)
    {
        currentResolutionIndex = index;
        // Debug.Log($"Resolution Index Selected: {index}");
    }

    // 应用选定的分辨率
    public void ApplyResolution()
    {
        if (currentResolutionIndex >= 0 && currentResolutionIndex < availableResolutions.Length)
        {
            Resolution selectedResolution = availableResolutions[currentResolutionIndex];
            
            Screen.SetResolution(selectedResolution.width, selectedResolution.height, FullScreenMode.Windowed,
                selectedResolution.refreshRateRatio);
            
            _matchSafeArea.OnResolutionChanged();
        }

        // ToggleSettingsPanel(); // 应用后关闭面板
    }

    // 切换设置面板的显示/隐藏状态
    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }
}
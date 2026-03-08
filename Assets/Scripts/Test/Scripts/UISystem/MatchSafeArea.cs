using UnityEngine;
using UnityEngine.UI; // 确保包含这个命名空间

public class MatchSafeArea : MonoBehaviour
{
    public Canvas canvas;
    private RectTransform panelRectTransform; // 当前UI元素的RectTransform
    private CanvasScaler canvasScaler; // 获取Canvas上的CanvasScaler组件

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        panelRectTransform = GetComponent<RectTransform>();
        canvasScaler = canvas.GetComponent<CanvasScaler>();
    }

    void Start()
    {
        ApplySafeArea();
    }

    // 当设备方向改变时（例如手机从竖屏到横屏），也会触发SafeArea的改变
    // 可以在这里重新应用安全区域
    void OnRectTransformDimensionsChange()
    {
        // 避免在编辑器模式下频繁调用
        if (Application.isPlaying)
        {
            ApplySafeArea();
        }
    }

    void ApplySafeArea()
    {
        if (panelRectTransform == null || canvas == null || canvasScaler == null) return;

        // 获取设备的安全区域
        Rect safeArea = Screen.safeArea;
        // Debug.Log($"Safe Area: {safeArea}");
        // Debug.Log($"Screen Size: {Screen.width}x{Screen.height}");

        // 获取Canvas Scaler的参考分辨率
        Vector2 referenceResolution = Vector2.zero;
        if (canvasScaler != null && canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            referenceResolution = canvasScaler.referenceResolution;
        }
        else
        {
            // 如果不是ScaleWithScreenSize模式，或者没有Canvas Scaler，则使用屏幕实际分辨率作为参考
            referenceResolution = new Vector2(Screen.width, Screen.height);
        }
        
        

        // 计算安全区域的左下角和右上角在参考分辨率下的坐标
        Vector2 minAnchorPoint = safeArea.min; // 像素坐标
        Vector2 maxAnchorPoint = safeArea.max; // 像素坐标

        // 将像素坐标转换为相对于屏幕宽高的0-1比例
        minAnchorPoint.x /= Screen.width;
        minAnchorPoint.y /= Screen.height;
        maxAnchorPoint.x /= Screen.width;
        maxAnchorPoint.y /= Screen.height;

        // 将0-1比例转换为参考分辨率下的像素值
        float xMinInRefRes = minAnchorPoint.x * referenceResolution.x;
        float yMinInRefRes = minAnchorPoint.y * referenceResolution.y;
        float xMaxInRefRes = maxAnchorPoint.x * referenceResolution.x;
        float yMaxInRefRes = maxAnchorPoint.y * referenceResolution.y;

        // 计算相对于参考分辨率的边距
        float leftPaddingInRefRes = xMinInRefRes;
        float rightPaddingInRefRes = referenceResolution.x - xMaxInRefRes;
        float bottomPaddingInRefRes = yMinInRefRes;
        float topPaddingInRefRes = referenceResolution.y - yMaxInRefRes;
        
        panelRectTransform.offsetMin = new Vector2(leftPaddingInRefRes, bottomPaddingInRefRes);
        panelRectTransform.offsetMax = new Vector2(-rightPaddingInRefRes, -topPaddingInRefRes);

    }

    // 如果你有自己的分辨率管理系统，当分辨率改变时调用此方法
    public void OnResolutionChanged()
    {
        ApplySafeArea();
    }
}
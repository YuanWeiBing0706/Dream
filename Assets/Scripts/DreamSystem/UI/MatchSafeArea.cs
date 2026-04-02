using UnityEngine;
using UnityEngine.UI;

namespace DreamSystem.UI
{
    public class MatchSafeArea : MonoBehaviour
    {
        /// 画布组件
        public Canvas canvas;
        /// 当前 UI 元素的矩形变换（RectTransform）
        private RectTransform panelRectTransform;
        /// 获取 Canvas 上的 CanvasScaler 组件
        private CanvasScaler canvasScaler;

        /// <summary>
        /// 初始化并获取所需的 UI 组件
        /// </summary>
        void Awake()
        {
            // 获取父级画布组件
            canvas = GetComponentInParent<Canvas>();
            panelRectTransform = GetComponent<RectTransform>();
            canvasScaler = canvas.GetComponent<CanvasScaler>();
        }

        /// <summary>
        /// 开始时应用一次安全区域适配
        /// </summary>
        void Start()
        {
            ApplySafeArea();
        }

        /// <summary>
        /// 当设备方向改变或矩形改变时，重新应用安全区域
        /// </summary>
        void OnRectTransformDimensionsChange()
        {
            // 避免在编辑器模式下频繁调用
            if (Application.isPlaying)
            {
                ApplySafeArea();
            }
        }

        /// <summary>
        /// 计算设备的异形屏安全区域并应用到当前 UI 的内边距
        /// </summary>
        void ApplySafeArea()
        {
            if (panelRectTransform == null || canvas == null || canvasScaler == null) return;

            // 获取设备的安全区域像素边框
            Rect safeArea = Screen.safeArea;

            // 获取 Canvas Scaler 的参考分辨率
            Vector2 referenceResolution = Vector2.zero;
            if (canvasScaler != null && canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                referenceResolution = canvasScaler.referenceResolution;
            }
            else
            {
                // 如果不是缩放模式，使用屏幕实际分辨率
                referenceResolution = new Vector2(Screen.width, Screen.height);
            }

            // 获取安全区域的屏幕像素坐标
            Vector2 minAnchorPoint = safeArea.min; 
            Vector2 maxAnchorPoint = safeArea.max; 

            // 将像素坐标转换为相对于屏幕宽高的比例 (0~1)
            minAnchorPoint.x /= Screen.width;
            minAnchorPoint.y /= Screen.height;
            maxAnchorPoint.x /= Screen.width;
            maxAnchorPoint.y /= Screen.height;

            // 将比例重新映射回参考分辨率下的对应像素值
            float xMinInRefRes = minAnchorPoint.x * referenceResolution.x;
            float yMinInRefRes = minAnchorPoint.y * referenceResolution.y;
            float xMaxInRefRes = maxAnchorPoint.x * referenceResolution.x;
            float yMaxInRefRes = maxAnchorPoint.y * referenceResolution.y;

            // 计算需要向内收缩的边距
            float leftPaddingInRefRes = xMinInRefRes;
            float rightPaddingInRefRes = referenceResolution.x - xMaxInRefRes;
            float bottomPaddingInRefRes = yMinInRefRes;
            float topPaddingInRefRes = referenceResolution.y - yMaxInRefRes;
        
            // 应用边距到 RectTransform
            panelRectTransform.offsetMin = new Vector2(leftPaddingInRefRes, bottomPaddingInRefRes);
            panelRectTransform.offsetMax = new Vector2(-rightPaddingInRefRes, -topPaddingInRefRes);

        }

        /// <summary>
        /// 当外部触发分辨率修改时调用的回调，用以刷新安全区域
        /// </summary>
        public void OnResolutionChanged()
        {
            ApplySafeArea();
        }
    }
}
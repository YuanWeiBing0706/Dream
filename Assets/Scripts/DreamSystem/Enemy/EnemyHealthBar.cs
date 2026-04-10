using UnityEngine;
using UnityEngine.UI;
using DreamSystem.Damage.Stat;

namespace DreamSystem.Enemy
{
    /// <summary>
    /// 怪物头顶血条。
    /// <para>挂到怪物根节点，Awake 时自动在头顶生成 WorldSpace Canvas 血条，始终朝向主摄像机。</para>
    /// <para>由 EnemyInjuriedSystem 在受伤时调用 UpdateHp()。</para>
    /// </summary>
    public class EnemyHealthBar : MonoBehaviour
    {
        [Tooltip("血条中心距怪物原点的高度偏移（米）")]
        [SerializeField] private float HeightOffset = 2.2f;

        [Tooltip("血条宽度（世界单位 = 米）")]
        [SerializeField] private float BarWidth = 1.5f;

        [Tooltip("血条高度（世界单位 = 米）")]
        [SerializeField] private float BarHeight = 0.12f;

        private Image _fillImage;
        private RectTransform _fillRect;
        private Transform _canvasRoot;
        private UnityEngine.Camera _cam;

        private void Awake()
        {
            _cam = UnityEngine.Camera.main;
            BuildBar();
        }

        private void BuildBar()
        {
            var canvasGo = new GameObject("HPBarCanvas");
            canvasGo.transform.SetParent(transform, false);
            canvasGo.transform.localPosition = new Vector3(0f, HeightOffset, 0f);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;

            var rt = canvasGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(BarWidth, BarHeight);
            _canvasRoot = canvasGo.transform;

            // 深色背景
            var bgGo = new GameObject("BG");
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bgRt = bgGo.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);

            // 红色血量填充
            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(canvasGo.transform, false);
            _fillRect = fillGo.AddComponent<RectTransform>();
            _fillRect.anchorMin = Vector2.zero;
            _fillRect.anchorMax = Vector2.one;
            _fillRect.offsetMin = _fillRect.offsetMax = Vector2.zero;

            _fillImage = fillGo.AddComponent<Image>();
            _fillImage.color = new Color(0.9f, 0.12f, 0.12f, 1f);
            // 不使用 Image.Type.Filled，改用 RectTransform 宽度缩放，避免某些材质/默认 sprite 下填充不生效
            _fillImage.type = Image.Type.Simple;
            SetFillRatio(1f);
        }

        private void LateUpdate()
        {
            // Billboard：使血条平面始终朝向摄像机
            if (_cam != null && _canvasRoot != null)
                _canvasRoot.rotation = _cam.transform.rotation;
        }

        /// <summary>
        /// 更新血条显示，由 EnemyInjuriedSystem 在每次伤害结算后调用。
        /// </summary>
        public void UpdateHp(float current, float max)
        {
            if (_fillRect == null) return;
            float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            SetFillRatio(ratio);
        }

        private void SetFillRatio(float ratio)
        {
            if (_fillRect == null) return;
            // 左侧固定，从右向左缩短
            _fillRect.anchorMax = new Vector2(ratio, 1f);
        }
    }
}

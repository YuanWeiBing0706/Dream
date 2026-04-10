using System;
using System.Text;
using Cysharp.Threading.Tasks;
using Data;
using DreamConfig;
using DreamManager;
using DreamSystem.Damage.Stat;
using Enum.Buff;
using Model.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace DreamSystem.UI
{
    /// <summary>
    /// 属性实时调试面板（仅用于开发测试，勿放入正式 Build）。
    /// <para>运行时自动创建 Canvas 和 UI 元素，无需在 Inspector 配置任何绑定。</para>
    /// <para>使用方式：在 Battle 场景中创建空 GameObject，挂上此脚本。</para>
    /// <para>然后在 MainGameScope 的 Inspector 里将该 GameObject 赋给 StatsDebugPanel 字段，
    /// 并在 Configure() 中添加一行 builder.RegisterComponent(_statsDebugPanel).AsSelf()。</para>
    /// <para>快捷键：默认 F1 切换显示/隐藏，Inspector 可修改。</para>
    /// </summary>
    public class StatsDebugPanel : MonoBehaviour
    {
        [Tooltip("切换面板显示的快捷键")]
        [SerializeField] private KeyCode _toggleKey = KeyCode.F1;

        [Tooltip("面板初始是否显示")]
        [SerializeField] private bool _startVisible = true;

        [Tooltip("调试面板文字字体（可选，不填则使用 TMP 默认字体）")]
        [SerializeField] private TMP_FontAsset _fontAsset;

        private PlayerModel _playerModel;
        private GameSessionData _sessionData;
        private ResourcesManager _resources;
        private TMP_Text _statsText;
        private GameObject _canvasRoot;
        private GameObject _panelRoot;
        private bool _isVisible;

        [Inject]
        public void Construct(GameSessionData sessionData, ResourcesManager resources)
        {
            _sessionData = sessionData;
            _resources   = resources;
        }

        private readonly StringBuilder _sb = new StringBuilder(512);

        // Unity 生命周期
        // ─────────────────────────────────────────────────────────────────

        private void Start()
        {
            _isVisible = _startVisible;
            BuildUI();
            FindAndSubscribeAsync().Forget();
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
                SetVisible(!_isVisible);
        }

        private void OnDestroy()
        {
            if (_playerModel?.Stats != null)
                _playerModel.Stats.OnDataChanged -= OnStatsChanged;

            if (_canvasRoot != null)
                Destroy(_canvasRoot);
        }

        // ─────────────────────────────────────────────────────────────────
        // 查找 PlayerModel 并订阅事件
        // ─────────────────────────────────────────────────────────────────

        private async UniTaskVoid FindAndSubscribeAsync()
        {
            // 等待 PlayerModel 出现在场景中
            await UniTask.WaitUntil(
                () => FindObjectOfType<PlayerModel>() != null,
                cancellationToken: destroyCancellationToken);

            _playerModel = FindObjectOfType<PlayerModel>();

            // 等待属性异步初始化完成
            await UniTask.WaitUntil(
                () => _playerModel != null && _playerModel.Stats != null,
                cancellationToken: destroyCancellationToken);

            _playerModel.Stats.OnDataChanged += OnStatsChanged;
            RefreshDisplay();
        }

        private void OnStatsChanged(int _) => RefreshDisplay();

        // ─────────────────────────────────────────────────────────────────
        // 刷新文本
        // ─────────────────────────────────────────────────────────────────

        private void RefreshDisplay()
        {
            if (_statsText == null || _playerModel?.Stats == null) return;

            _sb.Clear();

            _sb.AppendLine($"<b>── 属性调试面板 ({_toggleKey}切换) ──</b>");
            _sb.AppendLine();
            _sb.AppendLine("<b>  属性      基础值   最终值   差值</b>");
            _sb.AppendLine();

            AppendStatRow("生命", StatType.Health);
            AppendStatRow("护盾", StatType.Shield);
            AppendStatRow("攻击", StatType.Attack);
            AppendStatRow("防御", StatType.Defense);
            AppendStatRow("速度", StatType.Speed);

            // 当前血量单独显示
            _sb.AppendLine();
            _sb.AppendLine("<b>── 当前值 ──</b>");
            float curHp  = _playerModel.Stats.GetCurrentStatValue(StatType.Health);
            float maxHp  = _playerModel.Stats.GetStat(StatType.Health)?.FinalValue ?? 0f;
            float curShd = _playerModel.Stats.GetCurrentStatValue(StatType.Shield);
            _sb.AppendLine($"血量: <color=#ff6b6b>{curHp:F0} / {maxHp:F0}</color>");
            if (curShd > 0)
                _sb.AppendLine($"护盾: <color=#6bb8ff>{curShd:F0}</color>");

            // 修改器激活列表
            _sb.AppendLine();
            _sb.AppendLine("<b>── 激活的属性修改 ──</b>");
            bool anyMod = false;
            foreach (StatType st in System.Enum.GetValues(typeof(StatType)))
            {
                var bs = _playerModel.Stats.GetStat(st);
                if (bs == null) continue;
                float diff = bs.FinalValue - bs.BaseValue;
                if (Math.Abs(diff) > 0.001f)
                {
                    string c = diff > 0 ? "#44ff88" : "#ff5555";
                    _sb.AppendLine($"  <color={c}>{st}  {diff:+0.#;-0.#}</color>");
                    anyMod = true;
                }
            }
            if (!anyMod)
                _sb.AppendLine("  <color=#888888>（无）</color>");

            // 已获得的 Hex
            AppendOwnedHexSection();

            // 已获得的 Item
            AppendOwnedItemSection();

            _statsText.text = _sb.ToString();
        }

        private void AppendOwnedHexSection()
        {
            _sb.AppendLine();
            _sb.AppendLine("<b>── 已获得 Hex ──</b>");

            if (_sessionData == null || _sessionData.UnlockedHexIds == null || _sessionData.UnlockedHexIds.Count == 0)
            {
                _sb.AppendLine("  <color=#888888>（无）</color>");
                return;
            }

            var hexConfig = _resources?.GetConfig<HexConfig>();
            foreach (var hexId in _sessionData.UnlockedHexIds)
            {
                if (hexConfig != null && hexConfig.TryGet(hexId, out var hex))
                    _sb.AppendLine($"  <color=#cc99ff>[{hex.hexRarity}] {hex.hexName}</color>\n" +
                                   $"    <color=#999999>{TruncateDesc(hex.description)}</color>");
                else
                    _sb.AppendLine($"  <color=#888888>{hexId}（配置未找到）</color>");
            }
        }

        private void AppendOwnedItemSection()
        {
            _sb.AppendLine();
            _sb.AppendLine("<b>── 已获得 Item ──</b>");

            if (_sessionData == null || _sessionData.OwnedItemIds == null || _sessionData.OwnedItemIds.Count == 0)
            {
                _sb.AppendLine("  <color=#888888>（无）</color>");
                return;
            }

            var itemConfig = _resources?.GetConfig<ItemConfig>();
            foreach (var itemId in _sessionData.OwnedItemIds)
            {
                if (itemConfig != null && itemConfig.TryGet(itemId, out var item))
                    _sb.AppendLine($"  <color=#ffcc44>[{item.itemRarity}] {item.itemName}</color>\n" +
                                   $"    <color=#999999>{TruncateDesc(item.description)}</color>");
                else
                    _sb.AppendLine($"  <color=#888888>{itemId}（配置未找到）</color>");
            }
        }

        /// <summary>超过 40 字时截断并加省略号，防止面板溢出。</summary>
        private static string TruncateDesc(string desc)
        {
            if (string.IsNullOrEmpty(desc)) return "─";
            return desc.Length > 40 ? desc[..40] + "…" : desc;
        }

        private void AppendStatRow(string label, StatType type)
        {
            BaseStat stat = _playerModel.Stats.GetStat(type);
            if (stat == null) return;

            float diff = stat.FinalValue - stat.BaseValue;
            string diffColor;
            string diffText;
            if (diff > 0.001f)
            {
                diffColor = "#44ff88";
                diffText  = $"+{diff:F1}";
            }
            else if (diff < -0.001f)
            {
                diffColor = "#ff5555";
                diffText  = $"{diff:F1}";
            }
            else
            {
                diffColor = "#888888";
                diffText  = "─";
            }

            _sb.AppendLine(
                $"  {label,-4}  {stat.BaseValue,8:F1}  {stat.FinalValue,8:F1}  " +
                $"<color={diffColor}>{diffText}</color>");
        }

        // ─────────────────────────────────────────────────────────────────
        // 显示控制
        // ─────────────────────────────────────────────────────────────────

        private void SetVisible(bool visible)
        {
            _isVisible = visible;
            if (_panelRoot != null)
                _panelRoot.SetActive(visible);
        }

        // ─────────────────────────────────────────────────────────────────
        // 运行时动态创建 UI
        // ─────────────────────────────────────────────────────────────────

        private void BuildUI()
        {
            // Canvas（最高排序层，不受其他 Canvas 遮挡）
            _canvasRoot = new GameObject("[StatsDebugCanvas]");
            var canvas = _canvasRoot.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            var scaler = _canvasRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight  = 0.5f;

            _canvasRoot.AddComponent<GraphicRaycaster>();

            // 面板背景（右下角）
            _panelRoot = new GameObject("DebugPanel");
            _panelRoot.transform.SetParent(_canvasRoot.transform, false);

            var panelRt            = _panelRoot.AddComponent<RectTransform>();
            panelRt.anchorMin      = new Vector2(1, 0);
            panelRt.anchorMax      = new Vector2(1, 0);
            panelRt.pivot          = new Vector2(1, 0);
            panelRt.anchoredPosition = new Vector2(-12, 12);
            panelRt.sizeDelta      = new Vector2(380, 580);

            var bg = _panelRoot.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.04f, 0.08f, 0.88f);

            // 细边框（通过多一层 Image 实现）
            var borderGo = new GameObject("Border");
            borderGo.transform.SetParent(_panelRoot.transform, false);
            var borderRt            = borderGo.AddComponent<RectTransform>();
            borderRt.anchorMin      = Vector2.zero;
            borderRt.anchorMax      = Vector2.one;
            borderRt.offsetMin      = new Vector2(-1, -1);
            borderRt.offsetMax      = new Vector2(1, 1);
            var border              = borderGo.AddComponent<Image>();
            border.color            = new Color(0.4f, 0.6f, 1f, 0.3f);
            borderGo.transform.SetAsFirstSibling();

            // 文字区域（全内边距 12px）
            var textGo = new GameObject("StatsText");
            textGo.transform.SetParent(_panelRoot.transform, false);
            var textRt          = textGo.AddComponent<RectTransform>();
            textRt.anchorMin    = Vector2.zero;
            textRt.anchorMax    = Vector2.one;
            textRt.offsetMin    = new Vector2(12, 12);
            textRt.offsetMax    = new Vector2(-12, -12);

            _statsText                   = textGo.AddComponent<TextMeshProUGUI>();
            if (_fontAsset != null)
                _statsText.font = _fontAsset;
            _statsText.fontSize          = 14;
            _statsText.alignment         = TextAlignmentOptions.TopLeft;
            _statsText.richText          = true;
            _statsText.color             = new Color(0.9f, 0.9f, 0.9f, 1f);
            _statsText.enableWordWrapping = true;
            _statsText.overflowMode      = TextOverflowModes.Overflow;
            _statsText.text              = "<color=#888888>等待玩家属性初始化…</color>";

            _panelRoot.SetActive(_isVisible);
        }
    }
}

using System;
using System.Collections.Generic;
using RePuzzleKnights.Scripts.Domain.Enums;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies;
using RePuzzleKnights.Scripts.Presentation.InGame;
using UnityEngine;
using UnityEngine.UI;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.UI
{
    /// <summary>
    /// アークナイツ風キャラクター詳細パネル（画面左スライドイン）。
    ///
    /// ■ 構造 (全てプロシージャルに構築)
    ///   Root GO (this MonoBehaviour)
    ///     └─ DetailCanvas  [Canvas / CanvasScaler / GraphicRaycaster]
    ///          └─ Backdrop  [全画面透明 Button → 外クリックで閉じる]
    ///          └─ Panel     [320x480, 左端にスライドイン]
    ///               └─ 背景 / アクセントバー / 各UIパーツ
    ///
    /// ■ CleanArchitecture
    ///   View 層。外部ロジック（FusionUseCase 等）を一切持たない。
    ///   Presenter が Show(AllyDetailViewData) を呼んでデータを渡す。
    /// </summary>
    public class AllyDetailPanelView : MonoBehaviour, IAllyDetailPanelView
    {
        // ---- アニメーション ----
        private const float PANEL_W   = 320f;
        private const float PANEL_H   = 480f;
        private const float SHOWN_X   = 16f;
        private const float HIDDEN_X  = -(PANEL_W + 40f);
        private const float ANIM_SPEED = 16f;

        private RectTransform _panelRect;
        private CanvasGroup   _panelCG;
        private float         _targetX;

        // ---- UI パーツ ----
        private Text _nameText;
        private Text _levelText;
        private Text _elementText;
        private Text _placementText;
        private Text _hpValue;
        private Text _hpDiff;
        private Text _atkValue;
        private Text _atkDiff;
        private Text _blockValue;
        private Text _aspdValue;
        private Text _abilityTitle;
        private Text _abilityDesc;
        private readonly List<Image> _historyDots = new();
        private Image _accentBar;

        private Action _onBackdropClicked;
        private bool   _isBuilt;

        // 属性カラー
        private static readonly Dictionary<ElementType, Color> ElemColor = new()
        {
            { ElementType.Fire,   new Color(1.0f, 0.35f, 0.10f) },
            { ElementType.Water,  new Color(0.18f, 0.60f, 1.00f) },
            { ElementType.Grass,  new Color(0.25f, 0.88f, 0.35f) },
            { ElementType.Light,  new Color(1.00f, 0.92f, 0.20f) },
            { ElementType.Dark,   new Color(0.70f, 0.20f, 1.00f) },
            { ElementType.Normal, new Color(0.75f, 0.75f, 0.75f) },
        };

        // ======================================
        // Unity Lifecycle
        // ======================================

        private void Awake()
        {
            if (!_isBuilt) Build();
        }

        private void Update()
        {
            if (_panelRect == null) return;

            var pos = _panelRect.anchoredPosition;
            pos.x = Mathf.Lerp(pos.x, _targetX, Time.unscaledDeltaTime * ANIM_SPEED);
            _panelRect.anchoredPosition = pos;

            if (Input.GetMouseButtonDown(0) && IsShown() && !IsPointerInsidePanel() && !IsPointerOverAlly())
                _onBackdropClicked?.Invoke();
        }

        // ======================================
        // IAllyDetailPanelView
        // ======================================

        public void SetBackdropCallback(Action onBackdropClicked)
        {
            _onBackdropClicked = onBackdropClicked;
        }

        public void Show(AllyDetailViewData d)
        {
            if (!_isBuilt) Build();

            // --- ヘッダー ---
            _nameText.text = d.Name;
            string lvStr = d.IsEvolved
                ? "<color=#ffd700><b>MAX</b></color>"
                : $"Lv <b>{d.Level}</b>";
            _levelText.text = lvStr;

            Color ec = ElemColor.TryGetValue(d.Element, out var c) ? c : Color.white;
            _elementText.text  = GetElementName(d.Element);
            _elementText.color = ec;
            _accentBar.color   = new Color(ec.r, ec.g, ec.b, 0.85f);

            _placementText.text = d.IsHighGround ? "高台専用" : "地上専用";

            // --- ステータス ---
            _hpValue.text   = Mathf.RoundToInt(d.Hp).ToString();
            _atkValue.text  = Mathf.RoundToInt(d.Atk).ToString();
            _blockValue.text = d.Block.ToString();
            float aspd = d.AttackInterval > 0 ? Mathf.Round(1f / d.AttackInterval * 100f) / 100f : 0f;
            _aspdValue.text = aspd.ToString("F2") + "/s";

            // --- 強化差分 ---
            _hpDiff.text  = d.IsEvolved ? "" : FormatDiff(d.HpDiff);
            _atkDiff.text = d.IsEvolved ? "" : FormatDiff(d.AtkDiff);

            // --- スキル / アビリティ ---
            if (d.IsEvolved)
            {
                _abilityTitle.text = $"<b>{d.Name}</b>";
            }
            else if (d.IsNextEvolution && !string.IsNullOrEmpty(d.NextJobName))
            {
                _abilityTitle.text = $"<color=#ffd700>⇒ 次：{d.NextJobName}</color>";
            }
            else
            {
                _abilityTitle.text = "<b>アビリティ</b>";
            }
            _abilityDesc.text = d.AbilityDesc;

            // --- 履歴ドット ---
            for (int i = 0; i < _historyDots.Count; i++)
            {
                if (i < d.ElementHistory.Count)
                {
                    Color dc = ElemColor.TryGetValue(d.ElementHistory[i], out var hc) ? hc : Color.gray;
                    _historyDots[i].color = dc;
                    _historyDots[i].gameObject.SetActive(true);
                }
                else
                {
                    _historyDots[i].gameObject.SetActive(false);
                }
            }

            // スライドイン
            _targetX = SHOWN_X;
        }

        public void Hide()
        {
            _targetX = HIDDEN_X;
        }

        // ======================================
        // UI 構築
        // ======================================

        private void Build()
        {
            _isBuilt = true;

            // --- Root Canvas ---
            var canvasGO = new GameObject("AllyDetailCanvas");
            canvasGO.transform.SetParent(transform, false);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight  = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // --- Panel Root ---
            var panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            _panelRect = panelGO.AddComponent<RectTransform>();
            // 左下アンカー
            _panelRect.anchorMin = new Vector2(0, 0);
            _panelRect.anchorMax = new Vector2(0, 0);
            _panelRect.pivot     = new Vector2(0, 0);
            _panelRect.sizeDelta = new Vector2(PANEL_W, PANEL_H);
            _panelRect.anchoredPosition = new Vector2(HIDDEN_X, 20f);
            _targetX = HIDDEN_X;

            _panelCG       = panelGO.AddComponent<CanvasGroup>();
            _panelCG.blocksRaycasts = true;

            // パネル内クリックをワールドクリックとして扱わないためのレイキャスト面
            var panelBlocker = panelGO.AddComponent<Image>();
            panelBlocker.color = new Color(0, 0, 0, 0.001f);
            panelBlocker.raycastTarget = true;

            // ---- 背景レイヤー ----
            MakeFullRect(panelGO, "BG",       new Color(0.04f, 0.06f, 0.14f, 0.96f), 0);
            MakeFullRect(panelGO, "TopShade", new Color(0f, 0f, 0f, 0.30f),          1, topFrac: 0.58f);

            // ---- 左アクセントバー ----
            var accentGO  = new GameObject("AccentBar");
            accentGO.transform.SetParent(panelGO.transform, false);
            _accentBar    = accentGO.AddComponent<Image>();
            _accentBar.raycastTarget = false;
            var accentRT  = accentGO.GetComponent<RectTransform>();
            accentRT.anchorMin = new Vector2(0, 0);
            accentRT.anchorMax = new Vector2(0, 1);
            accentRT.pivot     = new Vector2(0, 0.5f);
            accentRT.sizeDelta = new Vector2(5f, 0);
            accentRT.anchoredPosition = Vector2.zero;

            // ---- コンテンツ ----
            float y   = PANEL_H - 16f;
            float pad = 18f;

            // 属性バッジ
            _elementText = MakeText(panelGO, "ElemBadge", "", 13, FontStyle.Bold, pad, y - 18f, PANEL_W - pad*2, 18f);
            y -= 20f;

            // キャラ名
            _nameText = MakeText(panelGO, "Name", "---", 24, FontStyle.Bold, pad, y - 34f, PANEL_W - pad*2, 34f);
            _nameText.color = Color.white;
            y -= 36f;

            // レベル + 配置タイプ
            _levelText = MakeText(panelGO, "Level", "Lv 1", 14, FontStyle.Normal, pad, y - 22f, 80f, 22f);
            _levelText.color = new Color(0.8f, 0.9f, 1f);
            _levelText.supportRichText = true;

            _placementText = MakeText(panelGO, "Placement", "地上専用", 12, FontStyle.Normal, pad + 90f, y - 22f, 120f, 22f);
            _placementText.color = new Color(0.55f, 0.75f, 0.95f);
            y -= 26f;

            // 区切り線
            y = Divider(panelGO, y - 6f, pad);

            // ステータスグリッド (2列)
            float col2 = pad + PANEL_W * 0.5f;
            float rowH = 28f;

            // HP
            MakeLabel(panelGO, "HPLbl", "HP",  pad,  y - rowH);
            _hpValue = MakeStatVal(panelGO, "HPVal", "---", pad + 36f, y - rowH, new Color(0.3f, 1f, 0.5f));
            _hpDiff  = MakeDiff(panelGO, "HPDiff", pad + 100f, y - rowH + 5f);
            y -= rowH;

            // ATK
            MakeLabel(panelGO, "ATKLbl", "ATK", pad, y - rowH);
            _atkValue = MakeStatVal(panelGO, "ATKVal", "---", pad + 40f, y - rowH, new Color(1f, 0.65f, 0.2f));
            _atkDiff  = MakeDiff(panelGO, "ATKDiff", pad + 104f, y - rowH + 5f);
            y -= rowH;

            // Block / ASPD
            MakeLabel(panelGO, "BlkLbl",  "Block", pad,     y - rowH);
            _blockValue = MakeStatVal(panelGO, "BlkVal",  "---", pad + 46f,   y - rowH, new Color(1f, 0.85f, 0.3f));
            MakeLabel(panelGO, "ASPDLbl", "ASPD",  col2,    y - rowH);
            _aspdValue  = MakeStatVal(panelGO, "ASPDVal", "---", col2 + 48f, y - rowH, new Color(0.7f, 0.85f, 1f));
            y -= rowH + 4f;

            // 区切り線
            y = Divider(panelGO, y - 4f, pad);

            // アビリティ
            _abilityTitle = MakeText(panelGO, "AblTitle", "アビリティ", 13, FontStyle.Bold, pad, y - 18f, PANEL_W - pad*2, 18f);
            _abilityTitle.color = new Color(0.85f, 0.85f, 1f);
            _abilityTitle.supportRichText = true;
            y -= 22f;

            _abilityDesc = MakeText(panelGO, "AblDesc", "", 11, FontStyle.Normal, pad, y - 68f, PANEL_W - pad*2, 68f);
            _abilityDesc.color         = new Color(0.78f, 0.82f, 0.95f);
            _abilityDesc.lineSpacing   = 1.25f;
            _abilityDesc.horizontalOverflow = HorizontalWrapMode.Wrap;
            _abilityDesc.verticalOverflow   = VerticalWrapMode.Overflow;
            y -= 74f;

            // 区切り線
            y = Divider(panelGO, y - 2f, pad);

            // 属性履歴ドット
            MakeText(panelGO, "HistLbl", "属性履歴", 11, FontStyle.Normal, pad, y - 16f, 64f, 16f).color = new Color(0.6f, 0.6f, 0.8f);
            for (int i = 0; i < 4; i++)
            {
                var dot = new GameObject($"Dot{i}");
                dot.transform.SetParent(panelGO.transform, false);
                var drt = dot.AddComponent<RectTransform>();
                drt.anchorMin = new Vector2(0,0); drt.anchorMax = new Vector2(0,0); drt.pivot = new Vector2(0,0);
                drt.sizeDelta = new Vector2(16f, 16f);
                drt.anchoredPosition = new Vector2(pad + 72f + i * 22f, y - 16f);
                var di = dot.AddComponent<Image>();
                di.raycastTarget = false;
                _historyDots.Add(di);
            }
        }

        private bool IsShown()
        {
            return _targetX > HIDDEN_X + 1f;
        }

        private bool IsPointerInsidePanel()
        {
            return _panelRect != null && RectTransformUtility.RectangleContainsScreenPoint(_panelRect, Input.mousePosition);
        }

        private static bool IsPointerOverAlly()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
                return false;

            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            foreach (var hit in Physics.RaycastAll(ray, Mathf.Infinity))
            {
                if (hit.collider.GetComponentInParent<AllyView>() != null)
                    return true;
            }

            return false;
        }

        // ======================================
        // UI ヘルパー
        // ======================================

        private static string GetElementName(ElementType e) => e switch
        {
            ElementType.Fire   => "🔥 Fire",
            ElementType.Water  => "💧 Water",
            ElementType.Grass  => "🌿 Grass",
            ElementType.Light  => "⚡ Light",
            ElementType.Dark   => "🌑 Dark",
            _                  => "Normal"
        };

        private static string FormatDiff(float v)
        {
            if (Mathf.Abs(v) < 0.5f) return "";
            string sign = v > 0 ? "+" : "";
            string col  = v > 0 ? "#44ff88" : "#ff5555";
            return $"<color={col}>{sign}{Mathf.RoundToInt(v)}</color>";
        }

        // フルサイズ背景矩形
        private static void MakeFullRect(GameObject parent, string name, Color color, int siblingIndex, float topFrac = 1f)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.SetSiblingIndex(siblingIndex);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            var rt  = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1f - topFrac);
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // 区切り線（水平）
        private static float Divider(GameObject parent, float y, float pad)
        {
            var go  = new GameObject("Div");
            go.transform.SetParent(parent.transform, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.3f, 0.4f, 0.6f, 0.45f);
            img.raycastTarget = false;
            var rt  = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0,0); rt.anchorMax = new Vector2(0,0); rt.pivot = new Vector2(0,0);
            rt.sizeDelta = new Vector2(PANEL_W - pad * 1.5f, 1f);
            rt.anchoredPosition = new Vector2(pad * 0.75f, y);
            return y - 10f;
        }

        // ラベル（灰色小文字）
        private Text MakeLabel(GameObject parent, string name, string text, float x, float y)
        {
            var t = MakeText(parent, name, text, 11, FontStyle.Normal, x, y, 44f, 22f);
            t.color = new Color(0.55f, 0.6f, 0.75f);
            return t;
        }

        // ステータス値（太字）
        private Text MakeStatVal(GameObject parent, string name, string text, float x, float y, Color color)
        {
            var t = MakeText(parent, name, text, 17, FontStyle.Bold, x, y, 64f, 24f);
            t.color = color;
            return t;
        }

        // 差分テキスト（小字・rich text対応）
        private Text MakeDiff(GameObject parent, string name, float x, float y)
        {
            var t = MakeText(parent, name, "", 12, FontStyle.Normal, x, y, 90f, 18f);
            t.color = Color.white;
            t.supportRichText = true;
            return t;
        }

        // テキスト生成ヘルパー
        private static Text MakeText(GameObject parent, string name, string text,
            int size, FontStyle style, float x, float y, float w, float h)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var t  = go.AddComponent<Text>();
            t.font       = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.text       = text;
            t.fontSize   = size;
            t.fontStyle  = style;
            t.color      = Color.white;
            t.supportRichText = false;
            t.raycastTarget   = false;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0,0); rt.anchorMax = new Vector2(0,0); rt.pivot = new Vector2(0,0);
            rt.sizeDelta        = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(x, y);
            return t;
        }
    }
}

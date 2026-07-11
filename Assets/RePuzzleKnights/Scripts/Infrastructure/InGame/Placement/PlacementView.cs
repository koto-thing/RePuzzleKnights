using Cysharp.Threading.Tasks;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Domain.Enums;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies;
using RePuzzleKnights.Scripts.Presentation.InGame;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.Placement
{
    public class PlacementView : MonoBehaviour, IPlacementView
    {
        [SerializeField] private Color validColor = new Color(0, 1, 0, 0.5f);
        [SerializeField] private Color invalidColor = new Color(1, 0, 0, 0.5f);
        
        private GameObject _currentPreviewObject;
        private Renderer[] _previewRenderers;
        private AssetReferenceGameObject _currentPrefabRef;

        // 現在の攻撃範囲インジケーター
        private List<GameObject> _rangeIndicators = new();
        private Material _rangeIndicatorMaterial;

        // 配置可能マスハイライト
        private List<GameObject> _placementHighlights = new();
        private Material _placementHighlightMaterial;

        // 融合プレビュー：次レベル攻撃範囲インジケーター（ゴールド色）
        private List<GameObject> _fusionRangeIndicators = new();
        private Material _fusionRangeIndicatorMaterial;

        // 融合プレビュー：スタット差分UIオーバーレイ
        private Canvas _fusionStatCanvas;
        private Text _fusionStatText;


        // ==============================
        // IPlacementView 実装
        // ==============================

        public void SetPreviewPrefab(AssetReferenceGameObject prefabRef)
        {
            _currentPrefabRef = prefabRef;
            SpawnPreviewAsync(prefabRef).Forget();
        }

        private async UniTaskVoid SpawnPreviewAsync(AssetReferenceGameObject prefabRef)
        {
            DestroyPreview();
            
            if (prefabRef == null || !prefabRef.RuntimeKeyIsValid())
            {
                Debug.LogError("PlacementView: Invalid PrefabRef. Cannot show preview.");
                return;
            }

            var handle = Addressables.InstantiateAsync(prefabRef);
            var obj = await handle.ToUniTask();

            _currentPreviewObject = obj;
            _currentPreviewObject.SetActive(false);
            
            var colliders = _currentPreviewObject.GetComponentsInChildren<Collider>();
            foreach (var col in colliders) col.enabled = false;
            
            var allyView = _currentPreviewObject.GetComponent<AllyView>();
            if (allyView != null) Destroy(allyView);
            
            _previewRenderers = _currentPreviewObject.GetComponentsInChildren<Renderer>();
        }

        public void ShowPreview(Vector3 position, Quaternion rotation, bool isValid,
            List<GridCoordinate> rangeGrids)
        {
            if (_currentPreviewObject != null)
            {
                _currentPreviewObject.SetActive(true);
                _currentPreviewObject.transform.position = position;
                _currentPreviewObject.transform.rotation = rotation;
                
                if (_previewRenderers != null)
                {
                    Color color = isValid ? validColor : invalidColor;
                    foreach (var r in _previewRenderers)
                    {
                        if (r.GetType().Name == "VisualEffectRenderer") continue;
                        foreach (var mat in r.materials)
                        {
                            if (mat.HasProperty("_Color")) mat.color = color;
                            else if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                        }
                    }
                }
            }

            UpdateRangeIndicators(position, rotation, isValid, rangeGrids);
        }

        public void HidePreview()
        {
            if (_currentPreviewObject != null)
                _currentPreviewObject.SetActive(false);

            foreach (var indicator in _rangeIndicators)
                if (indicator != null) indicator.SetActive(false);
        }

        // ==============================
        // 融合プレビュー：ShowFusionPreview
        // ==============================

        public void ShowFusionPreview(Vector3 targetPosition, Quaternion targetRotation,
            List<GridCoordinate> nextRangeGrids, float hpDiff, float atkDiff,
            bool isEvolution, string nextJobName)
        {
            // 次レベル攻撃範囲インジケーターの更新（ゴールド色）
            UpdateFusionRangeIndicators(targetPosition, targetRotation, nextRangeGrids);

            // スタット差分テキストUIの更新
            EnsureFusionStatUI();
            UpdateFusionStatText(targetPosition, hpDiff, atkDiff, isEvolution, nextJobName);
        }

        public void HideFusionPreview()
        {
            foreach (var ind in _fusionRangeIndicators)
                if (ind != null) ind.SetActive(false);

            if (_fusionStatCanvas != null)
                _fusionStatCanvas.gameObject.SetActive(false);
        }

        // ==============================
        // 有効配置マスハイライト
        // ==============================

        public void ShowValidPlacements(PlacementType placementType)
        {
            HideValidPlacements();

            int targetLayer = -1;
            if (placementType == PlacementType.Ground)
                targetLayer = LayerMask.NameToLayer("Ground");
            else if (placementType == PlacementType.HighGround)
                targetLayer = LayerMask.NameToLayer("HighGround");

            if (targetLayer == -1) return;

            var colliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
            var targetColliders = new List<Collider>();
            foreach (var col in colliders)
                if (col != null && col.gameObject.layer == targetLayer)
                    targetColliders.Add(col);

            if (_placementHighlightMaterial == null)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader != null)
                    _placementHighlightMaterial = new Material(shader);
            }

            Color highlightColor = new Color(0.4f, 1.0f, 0.4f, 0.15f);
            if (_placementHighlightMaterial != null)
                _placementHighlightMaterial.color = highlightColor;

            while (_placementHighlights.Count < targetColliders.Count)
            {
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Destroy(quad.GetComponent<Collider>());
                var renderer = quad.GetComponent<Renderer>();
                if (_placementHighlightMaterial != null && renderer != null)
                    renderer.material = _placementHighlightMaterial;
                quad.SetActive(false);
                _placementHighlights.Add(quad);
            }

            for (int i = 0; i < targetColliders.Count; i++)
            {
                var col = targetColliders[i];
                var highlight = _placementHighlights[i];
                if (col == null || highlight == null) continue;

                Vector3 pos = col.transform.position;
                pos.y = col.bounds.max.y + 0.02f;
                highlight.transform.position = pos;
                highlight.transform.rotation = Quaternion.Euler(90, 0, 0);
                Vector3 size = col.bounds.size;
                highlight.transform.localScale = new Vector3(size.x * 0.95f, size.z * 0.95f, 1.0f);
                highlight.SetActive(true);
            }
        }

        public void HideValidPlacements()
        {
            foreach (var highlight in _placementHighlights)
                if (highlight != null) highlight.SetActive(false);
        }

        // ==============================
        // 内部ヘルパー
        // ==============================

        private void UpdateRangeIndicators(Vector3 position, Quaternion rotation, bool isValid,
            List<GridCoordinate> rangeGrids)
        {
            foreach (var indicator in _rangeIndicators)
                if (indicator != null) indicator.SetActive(false);

            if (rangeGrids == null || rangeGrids.Count == 0) return;

            EnsureRangeIndicatorMaterial();

            while (_rangeIndicators.Count < rangeGrids.Count)
            {
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Destroy(quad.GetComponent<Collider>());
                var renderer = quad.GetComponent<Renderer>();
                if (_rangeIndicatorMaterial != null && renderer != null)
                    renderer.material = _rangeIndicatorMaterial;
                quad.SetActive(false);
                _rangeIndicators.Add(quad);
            }

            Color indicatorColor = isValid
                ? new Color(0.2f, 0.6f, 1.0f, 0.4f)
                : new Color(1.0f, 0.2f, 0.2f, 0.4f);

            if (_rangeIndicatorMaterial != null)
                _rangeIndicatorMaterial.color = indicatorColor;

            for (int i = 0; i < rangeGrids.Count; i++)
            {
                var coord = rangeGrids[i];
                var indicator = _rangeIndicators[i];
                if (indicator == null) continue;

                Vector3 localOffset = new Vector3(coord.X, 0, coord.Y);
                Vector3 worldPos = position + (rotation * localOffset);
                worldPos.y += 0.05f;
                indicator.transform.position = worldPos;
                indicator.transform.rotation = Quaternion.Euler(90, 0, 0);
                indicator.transform.localScale = new Vector3(0.9f, 0.9f, 1.0f);
                indicator.SetActive(true);
            }
        }

        private void UpdateFusionRangeIndicators(Vector3 position, Quaternion rotation,
            List<GridCoordinate> rangeGrids)
        {
            foreach (var ind in _fusionRangeIndicators)
                if (ind != null) ind.SetActive(false);

            if (rangeGrids == null || rangeGrids.Count == 0) return;

            if (_fusionRangeIndicatorMaterial == null)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    _fusionRangeIndicatorMaterial = new Material(shader);
                    // ゴールドカラー：進化後の攻撃範囲を金色で表示
                    _fusionRangeIndicatorMaterial.color = new Color(1.0f, 0.85f, 0.1f, 0.55f);
                }
            }

            while (_fusionRangeIndicators.Count < rangeGrids.Count)
            {
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Destroy(quad.GetComponent<Collider>());
                var renderer = quad.GetComponent<Renderer>();
                if (_fusionRangeIndicatorMaterial != null && renderer != null)
                    renderer.material = _fusionRangeIndicatorMaterial;
                quad.SetActive(false);
                _fusionRangeIndicators.Add(quad);
            }

            for (int i = 0; i < rangeGrids.Count; i++)
            {
                var coord = rangeGrids[i];
                var ind = _fusionRangeIndicators[i];
                if (ind == null) continue;

                Vector3 localOffset = new Vector3(coord.X, 0, coord.Y);
                Vector3 worldPos = position + (rotation * localOffset);
                worldPos.y += 0.06f; // 現レベル範囲より少し上に描画して重ねて見せる
                ind.transform.position = worldPos;
                ind.transform.rotation = Quaternion.Euler(90, 0, 0);
                // 強調のためにわずかに大きく
                ind.transform.localScale = new Vector3(0.95f, 0.95f, 1.0f);
                ind.SetActive(true);
            }
        }

        private void EnsureRangeIndicatorMaterial()
        {
            if (_rangeIndicatorMaterial == null)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader != null)
                    _rangeIndicatorMaterial = new Material(shader);
            }
        }

        /// <summary>融合ステータス差分を表示するワールドスペース Canvas を動的に生成</summary>
        private void EnsureFusionStatUI()
        {
            if (_fusionStatCanvas != null) return;

            var canvasGO = new GameObject("FusionStatPreview_Canvas");
            _fusionStatCanvas = canvasGO.AddComponent<Canvas>();
            _fusionStatCanvas.renderMode = RenderMode.WorldSpace;
            _fusionStatCanvas.worldCamera = Camera.main;
            _fusionStatCanvas.sortingOrder = 100;

            var cr = canvasGO.AddComponent<CanvasRenderer>();

            // 背景パネル
            var panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            var img = panelGO.AddComponent<Image>();
            img.color = new Color(0.05f, 0.05f, 0.12f, 0.82f);
            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(200, 80);

            // テキスト
            var textGO = new GameObject("StatText");
            textGO.transform.SetParent(panelGO.transform, false);
            _fusionStatText = textGO.AddComponent<Text>();
            _fusionStatText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _fusionStatText.fontSize = 18;
            _fusionStatText.alignment = TextAnchor.MiddleCenter;
            _fusionStatText.color = Color.white;
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(6, 4);
            textRect.offsetMax = new Vector2(-6, -4);

            // Canvas のスケールを 1unit = 100px に調整
            canvasGO.transform.localScale = Vector3.one * 0.01f;
        }

        private void UpdateFusionStatText(Vector3 worldPos, float hpDiff, float atkDiff,
            bool isEvolution, string nextJobName)
        {
            if (_fusionStatCanvas == null || _fusionStatText == null) return;

            // ワールド座標位置（ターゲットユニットの少し上）
            _fusionStatCanvas.transform.position = worldPos + new Vector3(0f, 2.0f, 0f);
            // カメラに向ける
            if (Camera.main != null)
                _fusionStatCanvas.transform.rotation = Camera.main.transform.rotation;

            // テキスト組み立て
            string hpStr = FormatDiff(hpDiff, "HP");
            string atkStr = FormatDiff(atkDiff, "ATK");
            string header = isEvolution ? $"<b>⇒ {nextJobName}</b>\n" : "<b>STRENGTHEN</b>\n";
            _fusionStatText.text = header + hpStr + "\n" + atkStr;

            _fusionStatCanvas.gameObject.SetActive(true);
        }

        private static string FormatDiff(float diff, string label)
        {
            if (Mathf.Abs(diff) < 0.01f) return $"{label}: <color=#aaaaaa>—</color>";
            string sign = diff >= 0 ? "+" : "";
            string color = diff >= 0 ? "#44ff88" : "#ff5555";
            return $"{label}: <color={color}>{sign}{diff:F0}</color>";
        }

        private void DestroyPreview()
        {
            if (_currentPreviewObject != null)
            {
                Destroy(_currentPreviewObject);
                _currentPreviewObject = null;
            }
            _previewRenderers = null;

            foreach (var indicator in _rangeIndicators)
                if (indicator != null) Destroy(indicator);
            _rangeIndicators.Clear();

            if (_rangeIndicatorMaterial != null)
            {
                Destroy(_rangeIndicatorMaterial);
                _rangeIndicatorMaterial = null;
            }

            foreach (var highlight in _placementHighlights)
                if (highlight != null) Destroy(highlight);
            _placementHighlights.Clear();

            if (_placementHighlightMaterial != null)
            {
                Destroy(_placementHighlightMaterial);
                _placementHighlightMaterial = null;
            }

            // 融合プレビューのクリーンアップ
            HideFusionPreview();
            foreach (var ind in _fusionRangeIndicators)
                if (ind != null) Destroy(ind);
            _fusionRangeIndicators.Clear();

            if (_fusionRangeIndicatorMaterial != null)
            {
                Destroy(_fusionRangeIndicatorMaterial);
                _fusionRangeIndicatorMaterial = null;
            }

            if (_fusionStatCanvas != null)
            {
                Destroy(_fusionStatCanvas.gameObject);
                _fusionStatCanvas = null;
                _fusionStatText = null;
            }
        }

        private void OnDestroy()
        {
            DestroyPreview();
        }
    }
}

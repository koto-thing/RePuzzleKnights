using System.Collections.Generic;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.UI
{
    /// <summary>
    /// ウェーブ開始時に敵の進行ルートを赤い点線のスクロールラインで可視化するプレビュークラス。
    /// </summary>
    public class PathPreviewRenderer : MonoBehaviour
    {
        private LineRenderer _lineRenderer;
        private Material _lineMaterial;
        private float _fadeTimer = 0f;
        private float _displayDuration = 6f; // 表示秒数 (6秒)
        private float _fadeDuration = 1.2f;   // フェードアウト秒数
        private bool _isFading = false;

        public static void Create(List<Vector3> points)
        {
            if (points == null || points.Count < 2) return;

            var go = new GameObject("PathPreview");
            var preview = go.AddComponent<PathPreviewRenderer>();
            preview.Setup(points);
        }

        private void Setup(List<Vector3> points)
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.positionCount = points.Count;
            
            // Zファイティングを避けるため、少しだけY座標を浮かす (0.15f)
            for (int i = 0; i < points.Count; i++)
            {
                _lineRenderer.SetPosition(i, points[i] + new Vector3(0f, 0.15f, 0f));
            }

            _lineRenderer.widthMultiplier = 0.25f;
            _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _lineRenderer.receiveShadows = false;
            _lineRenderer.useWorldSpace = true;

            // 初期頂点カラーは白（不透明）
            _lineRenderer.startColor = Color.white;
            _lineRenderer.endColor = Color.white;

            // 動的ドットテクスチャの生成
            Texture2D dotTexture = CreateDotTexture();

            // カスタムシェーダーを検索してロード
            Shader shader = Shader.Find("Custom/PathPreview");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            
            _lineMaterial = new Material(shader);
            _lineMaterial.mainTexture = dotTexture;

            _lineRenderer.material = _lineMaterial;

            // 経路の長さに基づいて点線のタイリング設定
            float totalDistance = 0f;
            for (int i = 0; i < points.Count - 1; i++)
            {
                totalDistance += Vector3.Distance(points[i], points[i + 1]);
            }
            _lineMaterial.mainTextureScale = new Vector2(totalDistance * 1.5f, 1f);
        }

        private Texture2D CreateDotTexture()
        {
            Texture2D tex = new Texture2D(16, 1, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;
            for (int x = 0; x < 16; x++)
            {
                Color c = (x < 8) ? Color.white : new Color(1f, 1f, 1f, 0f);
                tex.SetPixel(x, 0, c);
            }
            tex.Apply();
            return tex;
        }

        private void Update()
        {
            _fadeTimer += Time.deltaTime;
            if (_fadeTimer >= _displayDuration)
            {
                _isFading = true;
            }

            if (_isFading)
            {
                float t = (_fadeTimer - _displayDuration) / _fadeDuration;
                if (t >= 1f)
                {
                    Destroy(gameObject);
                    return;
                }
                
                // 頂点カラーのアルファ値を減衰させ、シェーダー側の表示と連動
                float alpha = Mathf.Lerp(1f, 0f, t);
                Color c = new Color(1f, 1f, 1f, alpha);
                _lineRenderer.startColor = c;
                _lineRenderer.endColor = c;
            }
        }

        private void OnDestroy()
        {
            if (_lineMaterial != null)
            {
                Destroy(_lineMaterial);
            }
        }
    }
}

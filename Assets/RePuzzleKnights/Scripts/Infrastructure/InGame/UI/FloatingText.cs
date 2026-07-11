using TMPro;
using DG.Tweening;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.UI
{
    /// <summary>
    /// ワールド座標上にダメージや回復の数値をポップアップ表示するクラス。
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        private TextMeshPro _textMesh;

        public static void Create(Vector3 position, string text, Color color)
        {
            var go = new GameObject("FloatingText", typeof(TextMeshPro), typeof(FloatingText));
            // 重なりを避けるために僅かにランダムオフセットを加える
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.4f, 0.4f), 
                Random.Range(0.2f, 0.5f), 
                Random.Range(-0.4f, 0.4f)
            );
            go.transform.position = position + randomOffset;
            
            var ft = go.GetComponent<FloatingText>();
            ft.Setup(text, color);
        }

        private void Setup(string text, Color color)
        {
            _textMesh = GetComponent<TextMeshPro>();
            _textMesh.text = text;
            _textMesh.color = color;
            _textMesh.fontSize = 1.8f;
            _textMesh.alignment = TextAlignmentOptions.Center;
            
            // 初期ビルボード回転
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }

            float duration = 0.8f;
            // 上方へふわっと移動
            transform.DOMoveY(transform.position.y + 1.2f, duration).SetEase(Ease.OutQuad);
            
            // アルファ値フェードアウト
            DOTween.To(() => _textMesh.color, x => _textMesh.color = x, new Color(color.r, color.g, color.b, 0f), duration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => Destroy(gameObject));
        }

        private void LateUpdate()
        {
            // 常にメインカメラの方向を向く
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }
    }
}

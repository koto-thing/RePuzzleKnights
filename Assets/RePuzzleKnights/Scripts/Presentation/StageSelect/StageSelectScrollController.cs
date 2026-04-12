using UnityEngine;
using UnityEngine.InputSystem;

namespace RePuzzleKnights.Scripts.Presentation.StageSelect
{
    /// <summary>
    /// マウスカーソルが画面上部・下部に入ったとき、指定したTransformをスクロールさせる
    /// Cinemachine使用時はVirtual CameraのTransformを割り当てる
    /// </summary>
    public class StageSelectScrollController : MonoBehaviour
    {
        [Header("スクロール対象")]
        [Tooltip("動かすTransform。Cinemachine使用時はVirtual CameraのGameObjectを割り当てる")]
        [SerializeField] private Transform scrollTarget;

        [Header("スクロール設定")]
        [Tooltip("スクロール速度（ワールド単位/秒）")]
        [SerializeField] private float scrollSpeed = 5f;

        [Tooltip("画面上端・下端の判定領域（画面高さに対する割合 0〜1）")]
        [SerializeField, Range(0.05f, 0.4f)] private float edgeThreshold = 0.1f;

        [Header("スクロール軸")]
        [Tooltip("動かすワールド空間の軸。Z軸なら (0,0,1)、Y軸なら (0,1,0)")]
        [SerializeField] private Vector3 scrollAxis = new Vector3(0f, 0f, 1f);

        [Header("スクロール範囲制限")]
        [Tooltip("scrollAxis 方向の最小値")]
        [SerializeField] private float minPosition = -10f;

        [Tooltip("scrollAxis 方向の最大値")]
        [SerializeField] private float maxPosition = 10f;

        private bool _scrollEnabled = true;

        public void SetScrollEnabled(bool isEnabled) => _scrollEnabled = isEnabled;

        private void Update()
        {
            if (!_scrollEnabled || scrollTarget == null)
                return;

            var mouse = Mouse.current;
            if (mouse == null)
                return;

            float mouseY = mouse.position.ReadValue().y;
            float screenHeight = Screen.height;
            float edgeSize = screenHeight * edgeThreshold;

            float direction = 0f;

            if (mouseY >= screenHeight - edgeSize)
            {
                float t = (mouseY - (screenHeight - edgeSize)) / edgeSize;
                direction = Mathf.Clamp01(t);
            }
            else if (mouseY <= edgeSize)
            {
                float t = (edgeSize - mouseY) / edgeSize;
                direction = -Mathf.Clamp01(t);
            }

            if (direction == 0f)
                return;

            Vector3 axis = scrollAxis.normalized;
            Vector3 pos = scrollTarget.position;
            float currentValue = Vector3.Dot(pos, axis);
            float newValue = Mathf.Clamp(currentValue + direction * scrollSpeed * Time.deltaTime, minPosition, maxPosition);
            scrollTarget.position = pos + axis * (newValue - currentValue);
        }
    }
}
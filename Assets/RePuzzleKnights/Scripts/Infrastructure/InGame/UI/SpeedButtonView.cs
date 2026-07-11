using R3;
using RePuzzleKnights.Scripts.Presentation.InGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame
{
    /// <summary>
    /// 倍速切り替えボタンのUI表示を管理するViewコンポーネント。
    /// </summary>
    public class SpeedButtonView : MonoBehaviour, ISpeedButtonView
    {
        [SerializeField] private Button speedButton;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private Image arrowIcon; // アークナイツ風の ">>" 矢印アイコン用（任意）

        public Observable<Unit> OnClick => _onClick;
        private readonly Subject<Unit> _onClick = new();

        private void Start()
        {
            if (speedButton == null)
            {
                speedButton = GetComponent<Button>();
            }

            if (speedButton != null)
            {
                speedButton.onClick.AddListener(() => _onClick.OnNext(Unit.Default));
            }
        }

        /// <summary>
        /// アークナイツを模し、ボタンテキスト（2X）とアイコンを
        /// 等速の時は半透明（グレー）、2倍速の時は通常（白ハイライト）にする
        /// </summary>
        public void UpdateSpeedVisual(float speed)
        {
            bool isActive = speed == 2.0f;
            Color color = isActive ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1.0f);

            if (speedText != null)
            {
                speedText.color = color;
            }
            if (arrowIcon != null)
            {
                arrowIcon.color = color;
            }
        }

        private void OnDestroy()
        {
            _onClick.OnCompleted();
            _onClick.Dispose();
        }
    }
}

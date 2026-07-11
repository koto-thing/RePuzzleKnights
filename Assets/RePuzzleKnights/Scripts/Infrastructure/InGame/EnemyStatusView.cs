using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame
{
    public class EnemyStatusView : MonoBehaviour
    {
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Canvas canvas;

        private Slider _backSlider;
        private Transform _cameraTransform;
        private float _maxHp;
        
        public void Start()
        {
            if (Camera.main != null)
                _cameraTransform = Camera.main.transform;

            if (hpSlider != null)
            {
                var backGo = Instantiate(hpSlider.gameObject, hpSlider.transform.parent);
                backGo.name = "BackSlider";
                _backSlider = backGo.GetComponent<Slider>();
                
                backGo.transform.SetAsFirstSibling();
                
                var fill = backGo.transform.Find("Fill Area/Fill")?.GetComponent<Image>();
                if (fill != null)
                {
                    fill.color = Color.white;
                }
                
                var background = backGo.transform.Find("Background")?.GetComponent<Image>();
                if (background != null)
                {
                    background.enabled = false;
                }
            }

            SetVisible(false);
        }

        public void LateUpdate()
        {
            if (_cameraTransform != null)
            {
                transform.rotation = _cameraTransform.rotation;
            }
        }

        public void SetMaxHp(float maxHp)
        {
            _maxHp = maxHp;
            if (hpSlider == null)
            {
                Debug.LogError("EnemyStatusView: hpSlider is not assigned.");
                return;
            }

            hpSlider.maxValue = maxHp;
            hpSlider.value = maxHp;

            if (_backSlider != null)
            {
                _backSlider.maxValue = maxHp;
                _backSlider.value = maxHp;
            }

            SetVisible(false);
        }

        public void UpdateHp(float currentHp)
        {
            if (hpSlider == null)
            {
                Debug.LogError("EnemyStatusView: hpSlider is not assigned.");
                return;
            }
            
            hpSlider.DOKill();
            hpSlider.DOValue(currentHp, 0.1f);

            if (_backSlider != null)
            {
                _backSlider.DOKill();
                _backSlider.DOValue(currentHp, 0.3f).SetDelay(0.2f);
            }

            if (currentHp >= _maxHp - 0.01f || currentHp <= 0.01f)
            {
                SetVisible(false);
            }
            else
            {
                SetVisible(true);
            }
        }
        
        private void SetVisible(bool visible)
        {
            if (canvas != null)
                canvas.enabled = visible;
            else
                gameObject.SetActive(visible);
        }
    }
}


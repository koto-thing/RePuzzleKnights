using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RePuzzleKnights.Scripts.InGame.Allies
{
    public class AllyStatusView : MonoBehaviour
    {
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Canvas canvas;

        private Transform cameraTransform;

        private void Start()
        {
            if (Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        private void LateUpdate()
        {
            if (cameraTransform != null)
                transform.rotation = cameraTransform.rotation;
        }

        public void SetMaxHp(float maxHp)
        {
            if (hpSlider == null)
            {
                Debug.LogWarning("AllyStatusView: HP Slider is not assigned.");
                return;
            }

            hpSlider.maxValue = maxHp;
            hpSlider.value = maxHp;

            SetVisible(true);
        }

        public void UpdateHp(float currentHp)
        {
            if (hpSlider == null)
            { 
                Debug.LogWarning("AllyStatusView: HP Slider is not assigned.");
                return;
            }
            
            hpSlider.DOValue(currentHp, 0.2f);
            SetVisible(!(currentHp <= 0.01f));
        }

        private void SetVisible(bool isVisible)
        {
            if (canvas != null)
                canvas.enabled = isVisible;
            else 
                gameObject.SetActive(isVisible);
        }
    }
}
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    public class EnemyStatusView : MonoBehaviour
    {
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Canvas canvas;

        private Transform cameraTransform;
        
        public void Start()
        {
            if (Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        public void LateUpdate()
        {
            transform.rotation = cameraTransform.rotation;
        }

        public void SetMaxHp(float maxHp)
        {
            if (hpSlider == null)
            {
                Debug.LogError("EnemyStatusView: hpSlider is not assigned.");
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
                Debug.LogError("EnemyStatusView: hpSlider is not assigned.");
                return;
            }
            
            hpSlider.DOValue(currentHp, 0.2f);

            SetVisible(!(currentHp <= 0.01f));
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
using System;
using Cysharp.Threading.Tasks;
using R3;
using RePuzzleKnights.Scripts.InGame.Allies.SO;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace RePuzzleKnights.Scripts.InGame.PlacementSystem
{
    public class PlacementConnector : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private AllyDataSO allyData;
        [SerializeField] private float redeployTime = 5.0f;
        [SerializeField] private CanvasGroup canvasGroup;

        private PlacementModel model;

        [Inject]
        public void Construct(PlacementModel model)
        {
            this.model = model;
        }

        private void Start()
        {
            if (model == null)
            {
                Debug.LogError("PlacementConnector: PlacementModel is null.");
                return;
            }

            model.OnPlacementConfirmed
                .Where(payload => payload.data == allyData)
                .Subscribe(_ => gameObject.SetActive(false))
                .AddTo(this);

            model.OnAllyDefeated
                .Where(data => data == allyData)
                .Subscribe(_ => StartCooldownRoutine().Forget())
                .AddTo(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (model == null)
            {
                Debug.LogError("PlacementConnector: PlacementModel is null.");
                return;
            }
            
            model.StartDragging(allyData);
        }

        private async UniTaskVoid StartCooldownRoutine()
        {
            gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                canvasGroup.alpha = 0.5f;
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(redeployTime), cancellationToken: this.GetCancellationTokenOnDestroy());

            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.alpha = 1.0f;
            }
        }
    }
}
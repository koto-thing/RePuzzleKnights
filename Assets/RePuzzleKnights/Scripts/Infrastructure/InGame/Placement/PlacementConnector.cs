using System;
using Cysharp.Threading.Tasks;
using R3;
using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO;
using RePuzzleKnights.Scripts.Presentation.InGame;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.Placement
{
    public class PlacementConnector : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private AllyDataSO allyData;
        [SerializeField] private float redeployTime = 5.0f;
        [SerializeField] private CanvasGroup canvasGroup;

        private PlacementController _controller;
        private PlacementUseCase _useCase;

        [Inject]
        public void Construct(PlacementController controller, PlacementUseCase useCase)
        {
            this._controller = controller;
            this._useCase = useCase;
        }

        private void Start()
        {
            if (_useCase == null) return;

            _useCase.OnPlacementConfirmed
                .Where(payload => payload.stats.Name == allyData.AllyName) // Check Name
                .Subscribe(_ => gameObject.SetActive(false))
                .AddTo(this);

            _useCase.OnAllyDefeated
                .Where(name => name == allyData.AllyName)
                .Subscribe(_ => StartCooldownRoutine().Forget())
                .AddTo(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_controller == null) return;
            _controller.StartPlacement(allyData);
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


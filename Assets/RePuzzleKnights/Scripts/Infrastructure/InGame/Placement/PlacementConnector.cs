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
        [SerializeField] private SoulDataSO soulData;
        [SerializeField] private float redeployTime = 5.0f;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private UnityEngine.UI.Image iconImage;

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
            if (soulData != null && iconImage != null)
            {
                iconImage.sprite = soulData.Icon;
            }

            _useCase.OnPlacementConfirmed
                .Where(payload => soulData != null && payload.stats.Name == soulData.AllyName)
                .Subscribe(_ => gameObject.SetActive(false))
                .AddTo(this);

            _useCase.OnAllyDefeated
                .Where(name => soulData != null && name == soulData.AllyName)
                .Subscribe(_ => StartCooldownRoutine().Forget())
                .AddTo(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_controller == null || soulData == null) return;
            _controller.StartPlacement(soulData);
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


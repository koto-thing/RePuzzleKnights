using System;
using Cysharp.Threading.Tasks;
using R3;
using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Enums;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO;
using RePuzzleKnights.Scripts.Presentation.InGame;
using TMPro;
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
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private TextMeshProUGUI costText;

        private PlacementController _controller;
        private PlacementUseCase _useCase;
        private SoulUseCase _soulUseCase;

        private bool _isCooldown = false;
        private int _currentSoulCount = 0;

        [Inject]
        public void Construct(PlacementController controller, PlacementUseCase useCase, SoulUseCase soulUseCase)
        {
            this._controller = controller;
            this._useCase = useCase;
            this._soulUseCase = soulUseCase;
        }

        private void Start()
        {
            if (_useCase == null) return;
            if (soulData != null)
            {
                if (iconImage != null)
                {
                    iconImage.sprite = soulData.Icon;
                }
                // 初期表示は所持数0
                if (costText != null)
                {
                    costText.text = "0";
                }
            }

            _useCase.OnAllyDefeated
                .Where(name => soulData != null && name == soulData.AllyName)
                .Subscribe(_ => StartCooldownRoutine().Forget())
                .AddTo(this);

            if (soulData != null && _soulUseCase != null)
            {
                _soulUseCase.GetSoulCount(soulData.Element)
                    .Subscribe(count =>
                    {
                        _currentSoulCount = count;
                        // アークナイツ風の左上コスト位置に、所持数（集めたSoulの数）を表示
                        if (costText != null)
                        {
                            costText.text = count.ToString();
                        }
                        if (countText != null)
                        {
                            countText.text = count.ToString();
                        }
                        UpdateVisualState();
                    })
                    .AddTo(this);
            }

            UpdateVisualState();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_controller == null || soulData == null || _soulUseCase == null) return;
            
            // 魂の配置に必要なコストは常に 1。所持数が 1 以上ある場合のみ配置を開始する
            if (_soulUseCase.CanConsumeSoul(soulData.Element, 1))
            {
                _controller.StartPlacement(soulData);
            }
        }

        private void UpdateVisualState()
        {
            if (soulData == null || canvasGroup == null) return;

            // 配置コストは常に 1。
            bool hasEnoughSoul = _soulUseCase != null && _soulUseCase.CanConsumeSoul(soulData.Element, 1);
            bool isUsable = !_isCooldown && hasEnoughSoul;

            canvasGroup.interactable = isUsable;
            canvasGroup.alpha = isUsable ? 1.0f : 0.5f;

            if (costText != null)
            {
                // 所持数が0（配置不可）の場合は赤くする
                costText.color = hasEnoughSoul ? Color.white : Color.red;
            }
        }

        private async UniTaskVoid StartCooldownRoutine()
        {
            gameObject.SetActive(true);
            _isCooldown = true;
            UpdateVisualState();
            
            await UniTask.Delay(TimeSpan.FromSeconds(redeployTime), cancellationToken: this.GetCancellationTokenOnDestroy());

            _isCooldown = false;
            UpdateVisualState();
        }
    }
}


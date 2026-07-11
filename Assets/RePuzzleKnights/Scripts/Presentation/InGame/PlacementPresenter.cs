using System;
using Cysharp.Threading.Tasks;
using R3;
using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Domain.Enums;
using RePuzzleKnights.Scripts.Infrastructure.InGame;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO;
using UnityEngine;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    public class PlacementPresenter : IStartable, IDisposable
    {
        private readonly PlacementUseCase _useCase;
        private readonly FusionUseCase _fusionUseCase;
        private readonly IPlacementView _view;
        private readonly AllyFactory _allyFactory;
        private readonly SoulUseCase _soulUseCase;
        private readonly CompositeDisposable _disposables = new();
        
        private AllyDataSO _currentAllyData;

        public PlacementPresenter(
            PlacementUseCase useCase,
            FusionUseCase fusionUseCase,
            IPlacementView view,
            AllyFactory allyFactory,
            SoulUseCase soulUseCase)
        {
            this._useCase = useCase;
            this._fusionUseCase = fusionUseCase;
            this._view = view;
            this._allyFactory = allyFactory;
            this._soulUseCase = soulUseCase;
        }
        
        public void SetCurrentAllyData(AllyDataSO data)
        {
            _currentAllyData = data;
        }

        public void Start()
        {
            // 配置状態の変化を購読
            _useCase.CurrentPlacementState
                .Subscribe(state =>
                {
                    if (state == PlacementState.DRAGGING)
                    {
                        var stats = _useCase.SelectedAlly.CurrentValue;
                        if (stats != null)
                            _view.ShowValidPlacements(stats.PlacementType);
                    }
                    else if (state == PlacementState.IDLE)
                    {
                        _view.HideValidPlacements();
                        _view.HideFusionPreview();
                    }
                })
                .AddTo(_disposables);

            // プレビュー位置・回転・有効フラグの変化を購読
            Observable.CombineLatest(
                _useCase.PreviewPosition,
                _useCase.PreviewRotation,
                _useCase.IsValidPosition,
                _useCase.IsFusionMode,
                _useCase.CurrentPlacementState,
                (pos, rot, isValid, isFusion, state) => new { pos, rot, isValid, isFusion, state }
            ).Subscribe(x =>
            {
                if (x.state == PlacementState.DRAGGING || x.state == PlacementState.ORIENTING)
                {
                    var rangeGrids = _useCase.SelectedAlly.CurrentValue?.AttackRangeGrids;
                    _view.ShowPreview(x.pos, x.rot, x.isValid, rangeGrids);

                    // 融合モード時にプレビューを計算して表示
                    if (x.isFusion && _useCase.TargetAllyObject != null && _currentAllyData != null)
                    {
                        var targetRef = _useCase.TargetAllyObject.GetComponentInParent<AllyReference>();
                        if (targetRef != null)
                        {
                            var draggingStats = _allyFactory.CreateStats(_currentAllyData);
                            var preview = _fusionUseCase.PreviewFusion(targetRef.Ally, draggingStats);
                            _view.ShowFusionPreview(
                                _useCase.TargetAllyObject.transform.position,
                                _useCase.TargetAllyObject.transform.rotation,
                                preview.NextAttackRangeGrids,
                                preview.HpDiff,
                                preview.AtkDiff,
                                preview.IsEvolution,
                                preview.NextJobName
                            );
                        }
                        else
                        {
                            _view.HideFusionPreview();
                        }
                    }
                    else
                    {
                        _view.HideFusionPreview();
                    }
                }
                else
                {
                    _view.HidePreview();
                    _view.HideFusionPreview();
                }
            }).AddTo(_disposables);
            
            _useCase.OnPlacementConfirmed.Subscribe(payload =>
            {
                if (_currentAllyData != null)
                {
                    if (_soulUseCase.ConsumeSoul(_currentAllyData.Element, 1))
                    {
                        if (payload.targetAlly != null)
                        {
                            ExecuteFusion(payload.targetAlly);
                        }
                        else
                        {
                            SpawnAllyAsync(_currentAllyData, payload.position, payload.rotation).Forget();
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[PlacementPresenter] Not enough soul. Required: 1 {_currentAllyData.Element}");
                        _currentAllyData = null;
                    }
                }
                _view.HidePreview();
                _view.HideFusionPreview();
            }).AddTo(_disposables);
            
            _useCase.OnCanceled.Subscribe(_ =>
            {
                _view.HidePreview();
                _view.HideFusionPreview();
                _currentAllyData = null;
            }).AddTo(_disposables);
        }

        private void ExecuteFusion(GameObject targetAllyObj)
        {
            var reference = targetAllyObj.GetComponentInParent<AllyReference>();
            if (reference != null && _currentAllyData != null)
            {
                var stats = _allyFactory.CreateStats(_currentAllyData);
                var draggingAlly = new Ally("temp", stats);

                // 最終進化済みには融合不可。CanFuseでガードして安全に弾く
                if (!_fusionUseCase.CanFuse(reference.Ally, draggingAlly))
                {
                    Debug.LogWarning("[PlacementPresenter] Target ally is fully evolved. Fusion blocked.");
                    _currentAllyData = null;
                    return;
                }

                _fusionUseCase.PerformFusion(reference.Ally, draggingAlly);
            }
            _currentAllyData = null;
        }
        
        private async UniTaskVoid SpawnAllyAsync(AllyDataSO data, Vector3 position, Quaternion rotation)
        {
            await _allyFactory.CreateAllyAsync(data, position, rotation);
            _currentAllyData = null;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}

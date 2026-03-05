using System;
using Cysharp.Threading.Tasks;
using R3;
using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Enums;
using RePuzzleKnights.Scripts.Infrastructure.InGame;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    public class PlacementPresenter : IStartable, IDisposable
    {
        private readonly PlacementUseCase _useCase;
        private readonly IPlacementView _view;
        private readonly AllyFactory _allyFactory;
        private readonly CompositeDisposable _disposables = new();
        
        private AllyDataSO _currentAllyData;

        public PlacementPresenter(PlacementUseCase useCase, IPlacementView view, AllyFactory allyFactory)
        {
            this._useCase = useCase;
            this._view = view;
            this._allyFactory = allyFactory;
        }
        
        public void SetCurrentAllyData(AllyDataSO data)
        {
            _currentAllyData = data;
        }

        public void Start()
        {
            Observable.CombineLatest(
                _useCase.PreviewPosition,
                _useCase.PreviewRotation,
                _useCase.IsValidPosition,
                _useCase.CurrentPlacementState,
                (pos, rot, isValid, state) => new { pos, rot, isValid, state }
            ).Subscribe(x =>
            {
                if (x.state == PlacementState.DRAGGING || x.state == PlacementState.ORIENTING)
                {
                    _view.ShowPreview(x.pos, x.rot, x.isValid);
                }
                else
                {
                    _view.HidePreview();
                }
            }).AddTo(_disposables);
            
            _useCase.OnPlacementConfirmed.Subscribe(payload =>
            {
                if (_currentAllyData != null)
                {
                    SpawnAllyAsync(_currentAllyData, payload.position, payload.rotation).Forget();
                }
                _view.HidePreview();
            }).AddTo(_disposables);
            
            _useCase.OnCanceled.Subscribe(_ =>
            {
                _view.HidePreview();
                _currentAllyData = null;
            }).AddTo(_disposables);
        }
        
        private async UniTaskVoid SpawnAllyAsync(AllyDataSO data, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation)
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

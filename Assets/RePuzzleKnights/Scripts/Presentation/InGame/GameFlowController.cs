using System;
using R3;
using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Enums;
using RePuzzleKnights.Scripts.Domain.Services;
using UnityEngine;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    public class GameFlowController : IInitializable, IDisposable
    {
        private readonly GameFlowUseCase _useCase;
        private readonly BaseStatusUseCase _baseStatusUseCase;
        private readonly ILevelStatusProvider _levelStatusProvider;
        
        private readonly CompositeDisposable _disposables = new();
        
        public GameFlowController(
            GameFlowUseCase useCase,
            BaseStatusUseCase baseStatusUseCase,
            ILevelStatusProvider levelStatusProvider)
        {
            this._useCase = useCase;
            this._baseStatusUseCase = baseStatusUseCase;
            this._levelStatusProvider = levelStatusProvider;
        }

        public void Initialize()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _baseStatusUseCase.OnBaseDestroyed
                .Subscribe(_ =>
                {
                    Debug.Log("[GameFlowController] Base destroyed - Game Over");
                    _useCase.TransitionState(GameResultState.GAME_OVER);
                })
                .AddTo(_disposables);
            
            Observable.CombineLatest(
                    _levelStatusProvider.IsAllWavesFinished,
                    _levelStatusProvider.ActiveEnemyCount,
                    (isWaveFinished, enemyCount) => new { isWaveFinished, enemyCount }
                )
                .Where(x => x.isWaveFinished && x.enemyCount == 0)
                .Take(1)
                .Subscribe(_ =>
                {
                    Debug.Log("[GameFlowController] All waves cleared and no enemies - Game Clear");
                    _useCase.TransitionState(GameResultState.GAME_CLEAR);
                })
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}



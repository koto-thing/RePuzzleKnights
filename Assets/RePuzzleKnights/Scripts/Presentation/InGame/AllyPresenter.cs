using System;
using R3;
using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Entities;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    public class AllyPresenter : IDisposable
    {
        private readonly Ally _ally;
        private readonly ProcessAttackUseCase _useCase;
        private readonly IAllyView _view;
        private readonly CompositeDisposable _disposables = new();

        public AllyPresenter(Ally ally, ProcessAttackUseCase useCase, IAllyView view)
        {
            this._ally = ally;
            this._useCase = useCase;
            this._view = view;
        }

        public void Initialize()
        {
            _ally.CurrentHp
                .Subscribe(hp => _view.UpdateHpBar(hp, _ally.Stats.MaxHp))
                .AddTo(_disposables);

            _ally.IsDead
                .Where(isDead => isDead)
                .Subscribe(_ => _view.PlayDieAnimation())
                .AddTo(_disposables);
            
            _useCase.OnAttackExecuted
                .Subscribe(_ => _view.PlayAttackAnimation())
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}



using System;
using R3;
using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Entities;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    public class EnemyPresenter : IInitializable, IDisposable
    {
        private readonly Enemy _enemy;
        private readonly IEnemyView _view;
        private readonly CompositeDisposable _disposables = new();

        private readonly BaseStatusUseCase _baseStatusUseCase;
        private readonly Action _onEnemyDefeated;

        public EnemyPresenter(
            Enemy enemy, 
            IEnemyView view,
            BaseStatusUseCase baseStatusUseCase,
            Action onEnemyDefeated)
        {
            this._enemy = enemy;
            this._view = view;
            this._baseStatusUseCase = baseStatusUseCase;
            this._onEnemyDefeated = onEnemyDefeated;
        }

        public void Initialize()
        {
            _enemy.CurrentHp
                .Subscribe(hp => _view.UpdateHp(hp, _enemy.Stats.MaxHp))
                .AddTo(_disposables);

            _enemy.IsDead
                .Where(isDead => isDead)
                .Subscribe(_ => 
                {
                    _view.PlayDeathEffect();
                    _onEnemyDefeated?.Invoke();
                })
                .AddTo(_disposables);

            _enemy.CurrentTarget
                .Subscribe(target => 
                {
                   _view.MoveTo(target, _enemy.Stats.MoveSpeed);
                })
                .AddTo(_disposables);

            _enemy.OnGoalReached
                .Subscribe(_ => 
                {
                    _baseStatusUseCase.TakeDamage(1);
                    _enemy.TakeDamage(_enemy.Stats.MaxHp * 10.0f);
                })
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}



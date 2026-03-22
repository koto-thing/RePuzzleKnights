using System;
using R3;
using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Presentation.InGame;
using UnityEngine;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.CleanArchitecture.InterfaceAdapter.InGame
{
    public class EnemyPresenter : IInitializable, IDisposable
    {
        private readonly Enemy enemy;
        private readonly IEnemyView view;
        private readonly CompositeDisposable disposables = new();

        private readonly BaseStatusUseCase baseStatusUseCase;
        private readonly Action onEnemyDefeated;

        public EnemyPresenter(
            Enemy enemy, 
            IEnemyView view,
            BaseStatusUseCase baseStatusUseCase,
            Action onEnemyDefeated)
        {
            this.enemy = enemy;
            this.view = view;
            this.baseStatusUseCase = baseStatusUseCase;
            this.onEnemyDefeated = onEnemyDefeated;
        }

        public void Initialize()
        {
            enemy.CurrentHp
                .Subscribe(hp => view.UpdateHp(hp, enemy.Stats.MaxHp))
                .AddTo(disposables);

            enemy.IsDead
                .Where(isDead => isDead)
                .Subscribe(_ => 
                {
                    view.PlayDeathEffect();
                    onEnemyDefeated?.Invoke();
                })
                .AddTo(disposables);

            enemy.CurrentTarget
                .Subscribe(target => 
                {
                   view.MoveTo(target, enemy.Stats.MoveSpeed);
                })
                .AddTo(disposables);

            enemy.OnGoalReached
                .Subscribe(_ => 
                {
                    baseStatusUseCase.TakeDamage(1);
                    enemy.TakeDamage(enemy.Stats.MaxHp * 10.0f);
                })
                .AddTo(disposables);
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}



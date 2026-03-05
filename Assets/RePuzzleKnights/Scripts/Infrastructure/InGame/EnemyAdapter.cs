using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.Interface;
using RePuzzleKnights.Scripts.Presentation.InGame;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame
{
    public class EnemyAdapter : IEnemyEntity
    {
        private readonly IEnemy _domainEnemy;
        private readonly IEnemyView _view;

        public EnemyAdapter(IEnemy domainEnemy, IEnemyView view)
        {
            this._domainEnemy = domainEnemy;
            this._view = view;
        }

        public bool IsDead => _domainEnemy.IsDead.CurrentValue;
        public bool IsFlying => _domainEnemy.IsFlying;
        public Vector3 Position => _domainEnemy.Position;

        public void TakeDamage(float damage)
        {
            _domainEnemy.TakeDamage(damage);
        }

        public void OnBlocked(IAllyEntity blocker)
        {
            _domainEnemy.OnBlocked(null);
        }

        public void OnReleased()
        {
            _domainEnemy.OnReleased();
        }
        
        public void SetTargetPosition(Vector3 targetPos, float duration)
        {
            _view.SetTargetPosition(targetPos, duration);
        }
    }
}


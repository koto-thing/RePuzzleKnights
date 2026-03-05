using RePuzzleKnights.Scripts.Domain.Entities;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    public class EnemyController
    {
        private readonly Enemy _enemy;

        public EnemyController(Enemy enemy)
        {
            this._enemy = enemy;
        }

        public void Tick(float deltaTime)
        {
            
        }
        
        public void UpdatePosition(Vector3 position)
        {
             _enemy.UpdatePosition(position);
        }

        public void NotifyArrivedAtTarget()
        {
            _enemy.OnArrivedAtTarget();
        }
    }
}



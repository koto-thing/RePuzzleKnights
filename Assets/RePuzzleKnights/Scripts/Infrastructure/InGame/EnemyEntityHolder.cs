using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame
{
    public class EnemyEntityHolder : MonoBehaviour
    {
        public IEnemyEntity Entity { get; private set; }
        
        public void SetEntity(IEnemyEntity entity)
        {
            Entity = entity;
        }
    }
}

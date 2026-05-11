using R3;
using RePuzzleKnights.Scripts.Domain.Enums;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Domain.Entities
{
    public interface IEnemy
    {
        ReadOnlyReactiveProperty<bool> IsDead { get; }
        bool IsFlying { get; }
        ElementType Element { get; }
        Vector3 Position { get; }
        
        void TakeDamage(float damage);
        void FallIntoPitfall();
        void OnBlocked(Ally blocker);
        void OnReleased();
    }
}




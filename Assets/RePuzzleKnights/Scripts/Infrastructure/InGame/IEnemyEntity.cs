using RePuzzleKnights.Scripts.Domain.Enums;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.Interface;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame
{
    public interface IEnemyEntity
    {
        bool IsDead { get; }
        bool IsFlying { get; }
        ElementType Element { get; }
        Vector3 Position { get; }
        
        void TakeDamage(float damage);
        void ApplyStatusEffect(StatusEffectType type, float duration, float value);
        void FallIntoPitfall();
        void OnBlocked(IAllyEntity blocker);
        void OnReleased();
        void SetTargetPosition(Vector3 targetPos, float duration);
    }
}


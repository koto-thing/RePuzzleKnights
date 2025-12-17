using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Enemies.Interface
{
    public interface IEnemyEntity
    {
        bool IsFlying { get; }
        bool IsDead { get; }
        Vector3 Position { get; }
        float DistanceToGoal { get; }

        void OnBlocked(MonoBehaviour blocker);
        void OnReleased();
        void TakeDamage(float damage);
    }
}
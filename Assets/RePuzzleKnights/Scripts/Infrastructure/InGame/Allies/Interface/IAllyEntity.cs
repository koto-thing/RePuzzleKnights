using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.Interface
{
    public interface IAllyEntity
    {
        Vector3 Position { get; }
        Vector3 FacingDirection { get; }
        bool IsDead { get; }
        AllyDataSO Data { get; }
        void TakeDamage(float damage);
    }
}



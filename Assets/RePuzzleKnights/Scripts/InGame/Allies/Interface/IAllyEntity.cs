using RePuzzleKnights.Scripts.InGame.Allies.SO;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Allies.Interface
{
    public interface IAllyEntity
    {
        Vector3 Position { get; }
        bool IsDead { get; }
        AllyDataSO Data { get; }
        void TakeDamage(float damage);
    }
}
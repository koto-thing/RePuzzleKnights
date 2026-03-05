using UnityEngine;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    public interface IEnemyView
    {
        void MoveTo(Vector3 target, float speed);
        void SetTargetPosition(Vector3 target, float duration);
        void UpdateHp(float current, float max);
        void PlayDamageEffect();
        void PlayDeathEffect();
        void DestroyActor();
    }
}



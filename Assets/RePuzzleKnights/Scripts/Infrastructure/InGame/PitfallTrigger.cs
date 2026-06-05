using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame
{
    public class PitfallTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var holder = other.GetComponent<EnemyEntityHolder>();
            if (holder == null) return;

            var entity = holder.Entity;
            if (entity == null || entity.IsDead || entity.IsFlying) return;

            entity.FallIntoPitfall();
        }
    }
}
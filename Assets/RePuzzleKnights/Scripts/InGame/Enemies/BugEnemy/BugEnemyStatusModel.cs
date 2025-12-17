using R3;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Enemies.BugEnemy
{
    public class BugEnemyStatusModel
    {
        public ReadOnlyReactiveProperty<float> CurrentHp => currentHp;
        private readonly ReactiveProperty<float> currentHp = new ();

        public ReadOnlyReactiveProperty<bool> IsBlocked => isBlocked;
        private readonly ReactiveProperty<bool> isBlocked = new (false);

        public ReadOnlyReactiveProperty<bool> IsDead => isDead;
        private readonly ReactiveProperty<bool> isDead = new (false);

        public float AttackCooldownTimer { get; set; } = 0.0f;

        public void Initialize(float maxHp)
        {
            currentHp.Value = maxHp;
            isDead.Value = false;
            isBlocked.Value = false;
            AttackCooldownTimer = 0.0f;
        }

        public void TakeDamage(float damage)
        {
            if (isDead.Value)
                return;

            currentHp.Value = Mathf.Max(0, currentHp.Value - damage);
            if (currentHp.Value <= 0)
            {
                isDead.Value = true;
            }
        }

        public void SetBlocked(bool isBlocked)
        {
            this.isBlocked.Value = isBlocked;
        }
    }
}
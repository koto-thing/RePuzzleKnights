using System.Collections.Generic;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Domain.Enums;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Domain.Services
{
    public class BattleService
    {
        public void ExecuteAttack(Ally attacker, IEnemy mainTarget, IEnumerable<IEnemy> potentialTargets)
        {
            ApplyDamage(attacker, mainTarget);
            
            if (attacker.Stats.RangeType != AttackRangeType.SINGLE_TARGET && attacker.Stats.SplashRadius > 0)
            {
                ApplySplashDamage(attacker, mainTarget, potentialTargets);
            }
        }

        private void ApplyDamage(Ally attacker, IEnemy target)
        {
            if (target != null && !target.IsDead.CurrentValue)
            {
                float multiplier = ElementChart.GetMultiplier(attacker.Stats.Element, target.Element);
                target.TakeDamage(attacker.Stats.AttackPower * multiplier);
            }
        }

        private void ApplySplashDamage(Ally attacker, IEnemy mainTarget, IEnumerable<IEnemy> potentialTargets)
        {
            Vector3 center = mainTarget.Position;
            float radiusSqr = attacker.Stats.SplashRadius * attacker.Stats.SplashRadius;

            foreach (var enemy in potentialTargets)
            {
                if (enemy == mainTarget || enemy == null || enemy.IsDead.CurrentValue)
                    continue;
                
                float distSqr = (enemy.Position - center).sqrMagnitude;
                if (distSqr <= radiusSqr)
                {
                    ApplyDamage(attacker, enemy);
                }
            }
        }
    }
}

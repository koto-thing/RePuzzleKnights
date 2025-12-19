using System.Collections.Generic;
using System.Linq;
using R3;
using RePuzzleKnights.Scripts.InGame.Allies.SO;
using RePuzzleKnights.Scripts.InGame.Enemies.Interface;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Allies
{
    public class AllyBattleModel
    {
        private readonly AllyDataSO allyData;

        public int EnemyLayerMask = LayerMask.GetMask("Default", "Enemy");

        public ReadOnlyReactiveProperty<float> CurrentHp => currentHp;
        private readonly ReactiveProperty<float> currentHp = new (0.0f);

        public ReadOnlyReactiveProperty<bool> IsDead => isDead;
        private readonly ReactiveProperty<bool> isDead = new (false);
        
        public ReadOnlyReactiveProperty<float> AttackTimer => attackTimer;
        private readonly ReactiveProperty<float> attackTimer = new(0f);
        
        public ReadOnlyReactiveProperty<List<IEnemyEntity>> EnemiesInSight => enemiesInSight;
        private readonly ReactiveProperty<List<IEnemyEntity>> enemiesInSight = new(new List<IEnemyEntity>());
        
        public ReadOnlyReactiveProperty<List<IEnemyEntity>> BlockedEnemies => blockedEnemies;
        private readonly ReactiveProperty<List<IEnemyEntity>> blockedEnemies = new(new List<IEnemyEntity>());
        
        public Observable<IList<IEnemyEntity>> OnAttackRequested => onAttackRequested;
        private readonly Subject<IList<IEnemyEntity>> onAttackRequested = new();
        
        public Observable<Unit> OnAttackCancelled => onAttackCancelled;
        private readonly Subject<Unit> onAttackCancelled = new();
        
        public AllyBattleModel(AllyDataSO data)
        {
            this.allyData = data;
            currentHp.Value = data.MaxHp;
        }

        public void TakeDamage(float damage)
        {
            if (isDead.Value)
                return;

            currentHp.Value -= damage;
            if (currentHp.Value <= 0.0f)
            {
                currentHp.Value = 0.0f;
                isDead.Value = true;
            }
        }
        
        public void UpdateAttackTimer(float deltaTime)
        {
            attackTimer.Value += deltaTime;
        }
        
        public void ResetAttackTimer()
        {
            attackTimer.Value = 0f;
        }

        public void SetEnemiesInSight(List<IEnemyEntity> enemies)
        {
            enemiesInSight.Value = enemies;
        }
        
        public void AddEnemyInSight(IEnemyEntity enemy)
        {
            if (!enemiesInSight.Value.Contains(enemy))
            {
                var newList = new List<IEnemyEntity>(enemiesInSight.Value) { enemy };
                enemiesInSight.Value = newList;
            }
        }
        
        public void RemoveEnemyInSight(IEnemyEntity enemy)
        {
            var newList = new List<IEnemyEntity>(enemiesInSight.Value);
            newList.Remove(enemy);
            enemiesInSight.Value = newList;
        }
        
        public void BlockEnemy(IEnemyEntity enemy)
        {
            if (!blockedEnemies.Value.Contains(enemy))
            {
                var newList = new List<IEnemyEntity>(blockedEnemies.Value) { enemy };
                blockedEnemies.Value = newList;
            }
        }
        
        public void UnblockEnemy(IEnemyEntity enemy)
        {
            var newList = new List<IEnemyEntity>(blockedEnemies.Value);
            newList.Remove(enemy);
            blockedEnemies.Value = newList;
        }
        
        public void CleanUpLists()
        {
            var cleanedSight = new List<IEnemyEntity>(enemiesInSight.Value);
            cleanedSight.RemoveAll(e => e == null || e.IsDead);
            enemiesInSight.Value = cleanedSight;
            
            var cleanedBlocked = new List<IEnemyEntity>(blockedEnemies.Value);
            cleanedBlocked.RemoveAll(e => e == null || e.IsDead);
            blockedEnemies.Value = cleanedBlocked;
        }
        
        public bool CanBlock()
        {
            return blockedEnemies.Value.Count < allyData.BlockCount;
        }
        
        public bool IsAttackReady()
        {
            return attackTimer.Value >= allyData.AttackInterval;
        }
        
        /// <summary>
        /// 最も優先度の高い敵を一体取得（単体攻撃用）
        /// </summary>
        /// <returns>優先度の高い敵</returns>
        public IEnemyEntity GetBestTarget(Vector3 myPosition)
        {
            // 
            var candidates = enemiesInSight.Value
                .Where(e => e != null && !e.IsDead)
                .ToList();

            if (candidates.Count == 0)
                return null;

            switch (allyData.Priority)
            {
                case AttackPriority.FLYING_PRIORITIZED:
                    var flyingEnemies = candidates.Where(e => e.IsFlying).ToList();
                    if (flyingEnemies.Count > 0)
                    {
                        return flyingEnemies[0];
                    }

                    return GetClosestEnemy(myPosition, flyingEnemies);
                
                case AttackPriority.BLOCK_ONLY:
                    var blocked = blockedEnemies.Value.Where(e => e != null && !e.IsDead).ToList();
                    if (blocked.Count > 0)
                    {
                        return GetClosestEnemy(myPosition, blocked);
                    }

                    return null;
                
                case AttackPriority.CLOSEST:
                default:
                    return GetClosestEnemy(myPosition, candidates);
            }
        }

        private IEnemyEntity GetClosestEnemy(Vector3 myPosition, List<IEnemyEntity> enemies)
        {
            if (enemies.Count == 0)
                return null;

            IEnemyEntity bestTarget = null;
            float minSqrDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                float sqrDist = (enemy.Position - myPosition).sqrMagnitude;
                if (sqrDist < minSqrDistance)
                {
                    minSqrDistance = sqrDist;
                    bestTarget = enemy;
                }
            }
            
            return bestTarget;
        }

        /// <summary>
        /// 範囲内のすべての有効な敵を取得（範囲攻撃用）
        /// </summary>
        /// <returns></returns>
        public IList<IEnemyEntity> GetAllTargets()
        {
            return enemiesInSight.Value
                .Where(e => e != null && !e.IsDead)
                .ToList();
        }
        
        /// <summary>
        /// 攻撃実行
        /// </summary>
        /// <param name="targets">攻撃対象のリスト</param>
        /// <param name="attackPower">攻撃力</param>
        public void RequestAttack(IList<IEnemyEntity> targets, float attackPower)
        {
            if (targets == null || targets.Count == 0)
                return;

            foreach (var target in targets)
            {
                if (target != null && !target.IsDead)
                {
                    target.TakeDamage(attackPower);
                }
            }
            
            onAttackRequested.OnNext(targets);
        }
        
        /// <summary>
        /// 攻撃キャンセルを通知
        /// </summary>
        public void CancelAttack()
        {
            onAttackCancelled.OnNext(Unit.Default);
        }
    }
}


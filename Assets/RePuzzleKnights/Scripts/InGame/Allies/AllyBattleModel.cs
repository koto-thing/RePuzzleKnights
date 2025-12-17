using System.Collections.Generic;
using R3;
using RePuzzleKnights.Scripts.InGame.Allies.SO;
using RePuzzleKnights.Scripts.InGame.Enemies.Interface;

namespace RePuzzleKnights.Scripts.InGame.Allies
{
    public class AllyBattleModel
    {
        private readonly AllyDataSO allyData;
        
        public ReadOnlyReactiveProperty<float> AttackTimer => attackTimer;
        private readonly ReactiveProperty<float> attackTimer = new(0f);
        
        public ReadOnlyReactiveProperty<List<IEnemyEntity>> EnemiesInSight => enemiesInSight;
        private readonly ReactiveProperty<List<IEnemyEntity>> enemiesInSight = new(new List<IEnemyEntity>());
        
        public ReadOnlyReactiveProperty<List<IEnemyEntity>> BlockedEnemies => blockedEnemies;
        private readonly ReactiveProperty<List<IEnemyEntity>> blockedEnemies = new(new List<IEnemyEntity>());
        
        public Observable<IEnemyEntity> OnAttackRequested => onAttackRequested;
        private readonly Subject<IEnemyEntity> onAttackRequested = new();
        
        public AllyBattleModel(AllyDataSO data)
        {
            this.allyData = data;
        }
        
        public void UpdateAttackTimer(float deltaTime)
        {
            attackTimer.Value += deltaTime;
        }
        
        public void ResetAttackTimer()
        {
            attackTimer.Value = 0f;
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
        
        public IEnemyEntity GetBestTarget()
        {
            var validEnemies = new List<IEnemyEntity>();
            foreach (var enemy in enemiesInSight.Value)
            {
                if (enemy != null && !enemy.IsDead)
                {
                    validEnemies.Add(enemy);
                }
            }
            
            if (validEnemies.Count == 0)
                return null;
            
            return validEnemies[0];
        }
        
        public void RequestAttack(IEnemyEntity target)
        {
            onAttackRequested.OnNext(target);
        }
    }
}


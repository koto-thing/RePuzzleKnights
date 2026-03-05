using System.Collections.Generic;
using System.Linq;
using R3;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.Allies
{
    public class AllyBattleModel
    {
        private readonly AllyDataSO _allyData;

        public int EnemyLayerMask = LayerMask.GetMask("Default", "Enemy");

        public ReadOnlyReactiveProperty<float> CurrentHp => _currentHp;
        private readonly ReactiveProperty<float> _currentHp = new (0.0f);

        public ReadOnlyReactiveProperty<bool> IsDead => _isDead;
        private readonly ReactiveProperty<bool> _isDead = new (false);
        
        public ReadOnlyReactiveProperty<float> AttackTimer => _attackTimer;
        private readonly ReactiveProperty<float> _attackTimer = new(0f);
        
        public ReadOnlyReactiveProperty<List<IEnemyEntity>> EnemiesInSight => _enemiesInSight;
        private readonly ReactiveProperty<List<IEnemyEntity>> _enemiesInSight = new(new List<IEnemyEntity>());
        
        public ReadOnlyReactiveProperty<List<IEnemyEntity>> BlockedEnemies => _blockedEnemies;
        private readonly ReactiveProperty<List<IEnemyEntity>> _blockedEnemies = new(new List<IEnemyEntity>());
        
        public Observable<IList<IEnemyEntity>> OnAttackRequested => _onAttackRequested;
        private readonly Subject<IList<IEnemyEntity>> _onAttackRequested = new();
        
        public Observable<Unit> OnAttackCancelled => _onAttackCancelled;
        private readonly Subject<Unit> _onAttackCancelled = new();
        
        public AllyBattleModel(AllyDataSO data)
        {
            this._allyData = data;
            _currentHp.Value = data.MaxHp;
        }

        public void TakeDamage(float damage)
        {
            if (_isDead.Value)
                return;

            _currentHp.Value -= damage;
            if (_currentHp.Value <= 0.0f)
            {
                _currentHp.Value = 0.0f;
                _isDead.Value = true;
            }
        }
        
        public void UpdateAttackTimer(float deltaTime)
        {
            _attackTimer.Value += deltaTime;
        }
        
        public void ResetAttackTimer()
        {
            _attackTimer.Value = 0f;
        }

        public void SetEnemiesInSight(List<IEnemyEntity> enemies)
        {
            _enemiesInSight.Value = enemies;
        }
        
        public void AddEnemyInSight(IEnemyEntity enemy)
        {
            if (!_enemiesInSight.Value.Contains(enemy))
            {
                var newList = new List<IEnemyEntity>(_enemiesInSight.Value) { enemy };
                _enemiesInSight.Value = newList;
            }
        }
        
        public void RemoveEnemyInSight(IEnemyEntity enemy)
        {
            var newList = new List<IEnemyEntity>(_enemiesInSight.Value);
            newList.Remove(enemy);
            _enemiesInSight.Value = newList;
        }
        
        public void BlockEnemy(IEnemyEntity enemy)
        {
            if (!_blockedEnemies.Value.Contains(enemy))
            {
                var newList = new List<IEnemyEntity>(_blockedEnemies.Value) { enemy };
                _blockedEnemies.Value = newList;
            }
        }
        
        public void UnblockEnemy(IEnemyEntity enemy)
        {
            var newList = new List<IEnemyEntity>(_blockedEnemies.Value);
            newList.Remove(enemy);
            _blockedEnemies.Value = newList;
        }
        
        public void CleanUpLists()
        {
            var cleanedSight = new List<IEnemyEntity>(_enemiesInSight.Value);
            cleanedSight.RemoveAll(e => e == null || e.IsDead);
            _enemiesInSight.Value = cleanedSight;
            
            var cleanedBlocked = new List<IEnemyEntity>(_blockedEnemies.Value);
            cleanedBlocked.RemoveAll(e => e == null || e.IsDead);
            _blockedEnemies.Value = cleanedBlocked;
        }
        
        public bool CanBlock()
        {
            return _blockedEnemies.Value.Count < _allyData.BlockCount;
        }
        
        public bool IsAttackReady()
        {
            return _attackTimer.Value >= _allyData.AttackInterval;
        }
        
        /// <summary>
        /// 最も優先度の高い敵を一体取得（単体攻撃用）
        /// </summary>
        /// <returns>優先度の高い敵</returns>
        public IEnemyEntity GetBestTarget(Vector3 myPosition)
        {
            // 
            var candidates = _enemiesInSight.Value
                .Where(e => e != null && !e.IsDead)
                .ToList();

            if (candidates.Count == 0)
                return null;

            switch (_allyData.Priority)
            {
                case AttackPriority.FLYING_PRIORITIZED:
                    var flyingEnemies = candidates.Where(e => e.IsFlying).ToList();
                    if (flyingEnemies.Count > 0)
                    {
                        return flyingEnemies[0];
                    }

                    return GetClosestEnemy(myPosition, flyingEnemies);
                
                case AttackPriority.BLOCK_ONLY:
                    var blocked = _blockedEnemies.Value.Where(e => e != null && !e.IsDead).ToList();
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
            return _enemiesInSight.Value
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
            
            _onAttackRequested.OnNext(targets);
        }
        
        /// <summary>
        /// 攻撃キャンセルを通知
        /// </summary>
        public void CancelAttack()
        {
            _onAttackCancelled.OnNext(Unit.Default);
        }
    }
}





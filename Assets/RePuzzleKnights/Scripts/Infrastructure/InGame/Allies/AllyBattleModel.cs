using System.Collections.Generic;
using System.Linq;
using R3;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Domain.Enums;
using RePuzzleKnights.Scripts.Domain.Services;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.Allies
{
    public class AllyBattleModel
    {
        private readonly AllyDataSO _allyData;
        public AllyDataSO AllyData => _allyData;

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

        // 動的ステータス
        private float _maxHp;
        private float _attackPower;
        private float _attackInterval;
        private int _blockCount;

        // 攻撃範囲（グリッド）
        private List<GridCoordinate> _attackRangeGrids = new();
        public List<GridCoordinate> AttackRangeGrids => _attackRangeGrids;

        // アビリティ値
        private StatusEffectType _customEffectType = StatusEffectType.None;
        private float _customEffectDuration = 0f;
        private float _customEffectValue = 0f;
        private float _customEffectProbability = 0f;
        private float _selfRegenPercent = 0f;
        private float _reflectDamagePercent = 0f;
        private float _dodgeChance = 0f;
        private int _maxTargets = 1;
        private bool _isSplash = false;
        
        public AllyBattleModel(AllyDataSO data)
        {
            this._allyData = data;
            
            _maxHp = data.MaxHp;
            _attackPower = data.AttackPower;
            _attackInterval = data.AttackInterval;
            _blockCount = data.BlockCount;
            
            _currentHp.Value = data.MaxHp;

            // 初期攻撃範囲グリッド
            _attackRangeGrids = new List<GridCoordinate>();
            if (data.AttackRangeGrids != null)
            {
                foreach (var grid in data.AttackRangeGrids)
                {
                    _attackRangeGrids.Add(new GridCoordinate(grid.x, grid.y));
                }
            }

            // 初期アビリティ設定 (Factoryと同様の設定をフォールバックとして保持)
            InitializeDefaultAbilities(data.Element);
        }

        private void InitializeDefaultAbilities(ElementType element)
        {
            switch (element)
            {
                case ElementType.Fire:
                    _customEffectType = StatusEffectType.Burn;
                    _customEffectDuration = 3f;
                    _customEffectValue = 10f;
                    _customEffectProbability = 1f;
                    break;
                case ElementType.Water:
                    _customEffectType = StatusEffectType.Slow;
                    _customEffectDuration = 2f;
                    _customEffectValue = 0.3f;
                    _customEffectProbability = 1f;
                    break;
                case ElementType.Grass:
                    _selfRegenPercent = 0.01f;
                    _reflectDamagePercent = 0.1f;
                    _blockCount = 1; // Lv1
                    break;
                case ElementType.Light:
                    _customEffectType = StatusEffectType.Stun;
                    _customEffectDuration = 0.5f;
                    _customEffectProbability = 0.1f;
                    _maxTargets = 1;
                    break;
                case ElementType.Dark:
                    _dodgeChance = 0.2f;
                    _customEffectType = StatusEffectType.DefDebuff;
                    _customEffectDuration = 3f;
                    _customEffectValue = 10f;
                    _customEffectProbability = 0.5f;
                    _blockCount = 1;
                    break;
            }
        }

        public void TakeDamage(float damage)
        {
            if (_isDead.Value)
                return;

            // 物理回避 (Darkアビリティ)
            if (_dodgeChance > 0f && Random.value < _dodgeChance)
            {
                Debug.Log($"<color=purple>[Dodge]</color> Dodge active! Damage {damage} reduced to 0.");
                return;
            }

            _currentHp.Value -= damage;

            // 反射 (Grassアビリティ)
            if (_reflectDamagePercent > 0f && damage > 0f)
            {
                var enemies = _blockedEnemies.Value;
                if (enemies != null && enemies.Count > 0)
                {
                    float reflectDmg = damage * _reflectDamagePercent;
                    foreach (var enemy in enemies)
                    {
                        if (enemy != null && !enemy.IsDead)
                        {
                            enemy.TakeDamage(reflectDmg);
                        }
                    }
                }
            }

            if (_currentHp.Value <= 0.0f)
            {
                _currentHp.Value = 0.0f;
                _isDead.Value = true;
            }
        }

        public void ApplyRegen(float deltaTime)
        {
            if (_isDead.Value || _selfRegenPercent <= 0f) return;
            
            float regenAmount = _maxHp * _selfRegenPercent * deltaTime;
            _currentHp.Value = Mathf.Min(_maxHp, _currentHp.Value + regenAmount);
        }

        public void UpdateStats(AllyStats newStats)
        {
            _maxHp = newStats.MaxHp;
            _attackPower = newStats.AttackPower;
            _attackInterval = newStats.AttackInterval;
            _blockCount = newStats.BlockCount;
            
            _attackRangeGrids = newStats.AttackRangeGrids;

            // アビリティ値の更新
            _customEffectType = newStats.CustomEffectType;
            _customEffectDuration = newStats.CustomEffectDuration;
            _customEffectValue = newStats.CustomEffectValue;
            _customEffectProbability = newStats.CustomEffectProbability;
            _selfRegenPercent = newStats.SelfRegenPercent;
            _reflectDamagePercent = newStats.ReflectDamagePercent;
            _dodgeChance = newStats.DodgeChance;
            _maxTargets = newStats.MaxTargets;
            _isSplash = newStats.IsSplash;

            // HP回復
            _currentHp.Value = newStats.MaxHp;
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
            return _blockedEnemies.Value.Count < _blockCount;
        }
        
        public bool IsAttackReady()
        {
            return _attackTimer.Value >= _attackInterval;
        }
        
        /// <summary>
        /// 最も優先度の高い敵を一体取得（単体攻撃用）
        /// </summary>
        /// <returns>優先度の高い敵</returns>
        public IEnemyEntity GetBestTarget(Vector3 myPosition)
        {
            var candidates = _enemiesInSight.Value
                .Where(e => e != null && !e.IsDead)
                .ToList();
 
            if (candidates.Count == 0)
                return null;
 
            switch (_allyData.Priority)
            {
                case SO.AttackPriority.FLYING_PRIORITIZED:
                    var flyingEnemies = candidates.Where(e => e.IsFlying).ToList();
                    if (flyingEnemies.Count > 0)
                    {
                        return GetClosestEnemy(myPosition, flyingEnemies);
                    }
                    return GetClosestEnemy(myPosition, candidates);
                
                case SO.AttackPriority.BLOCK_ONLY:
                    var blocked = _blockedEnemies.Value.Where(e => e != null && !e.IsDead).ToList();
                    if (blocked.Count > 0)
                    {
                        return GetClosestEnemy(myPosition, blocked);
                    }
                    return null;
                
                case SO.AttackPriority.CLOSEST:
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
 
            // 動的更新後の攻撃力を使用
            float finalPower = _attackPower > 0f ? _attackPower : attackPower;
            int targetCount = 0;
 
            foreach (var target in targets)
            {
                if (target != null && !target.IsDead)
                {
                    float multiplier = ElementChart.GetMultiplier(_allyData.Element, target.Element);
                    float damage = finalPower * multiplier;

                    // 防御力デバフによる物理ダメージの増加（簡略計算）
                    if (target is MonoBehaviour mono)
                    {
                        var sem = mono.GetComponent<StatusEffectManager>();
                        if (sem != null)
                        {
                            damage += sem.GetDefenseDebuffAmount();
                        }
                    }

                    target.TakeDamage(damage);

                    // アビリティ状態異常（火傷、減速、スタン、防御デバフ）の適用
                    if (_customEffectType != StatusEffectType.None && Random.value < _customEffectProbability)
                    {
                        target.ApplyStatusEffect(_customEffectType, _customEffectDuration, _customEffectValue);
                    }

                    targetCount++;
                    
                    // 攻撃対象数の上限制限（魔法使いなどで適用。0や負の値は制限なしとみなす）
                    if (_maxTargets > 0 && targetCount >= _maxTargets)
                    {
                        break;
                    }
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





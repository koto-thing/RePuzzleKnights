using System.Collections.Generic;
using R3;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Domain.Entities
{
    public class Ally
    {
        public string Id { get; }
        public AllyStats Stats { get; private set; }
        public FusionState FusionState { get; }

        public Observable<AllyStats> OnStatsUpdated => _onStatsUpdated;
        private readonly Subject<AllyStats> _onStatsUpdated = new();

        public ReadOnlyReactiveProperty<float> CurrentHp => currentHp;
        private readonly ReactiveProperty<float> currentHp;

        public ReadOnlyReactiveProperty<bool> IsDead => isDead;
        private readonly ReactiveProperty<bool> isDead = new(false);

        public ReadOnlyReactiveProperty<float> AttackTimer => attackTimer;
        private readonly ReactiveProperty<float> attackTimer = new(0f);
        
        private readonly List<IEnemy> blockedEnemies = new();
        public IReadOnlyList<IEnemy> BlockedEnemies => blockedEnemies;

        public Ally(string id, AllyStats stats) 
        {
            Id = id;
            Stats = stats;
            FusionState = new FusionState(stats.Element);
            currentHp = new ReactiveProperty<float>(stats.MaxHp);
        }

        public void UpdateStats(AllyStats newStats)
        {
            Stats = newStats;
            // 最大HPの増加に合わせて現在HPをフル回復させる
            currentHp.Value = Stats.MaxHp;
            
            _onStatsUpdated.OnNext(Stats);
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="damage">受けるダメージ</param>
        public void TakeDamage(float damage)
        {
            if (isDead.Value) 
                return;

            currentHp.Value = Mathf.Max(0f, currentHp.Value - damage);
            if (currentHp.Value <= 0f)
            {
                isDead.Value = true;
                ReleaseAllBlockedEnemies();
            }
        }

        /// <summary>
        /// タイマーをアップデート
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdateTimer(float deltaTime)
        {
            if (isDead.Value) 
                return;
            
            attackTimer.Value += deltaTime;
        }

        /// <summary>
        /// 攻撃時間をアップデート
        /// </summary>
        public void ResetAttackTimer()
        {
            attackTimer.Value = 0f;
        }

        /// <summary>
        /// 攻撃できるかどうか
        /// </summary>
        /// <returns>できる場合: true</returns>
        public bool CanAttack()
        {
            return !isDead.Value && attackTimer.Value >= Stats.AttackInterval;
        }

        /// <summary>
        /// ブロックできるかどうか
        /// </summary>
        /// <returns>できる場合: true</returns>
        public bool CanBlock()
        {
            return !isDead.Value && blockedEnemies.Count < Stats.BlockCount;
        }

        /// <summary>
        /// 敵をブロックする
        /// </summary>
        /// <param name="enemy">ブロックする敵</param>
        public void BlockEnemy(IEnemy enemy)
        {
            if (CanBlock() && !blockedEnemies.Contains(enemy))
            {
                blockedEnemies.Add(enemy);
                enemy.OnBlocked(this);
            }
        }

        /// <summary>
        /// 敵をブロックから外す
        /// </summary>
        /// <param name="enemy">外す敵</param>
        public void ReleaseEnemy(IEnemy enemy)
        {
            if (blockedEnemies.Contains(enemy))
            {
                blockedEnemies.Remove(enemy);
                enemy.OnReleased();
            }
        }

        /// <summary>
        /// すべての敵をブロック状態から外す
        /// </summary>
        private void ReleaseAllBlockedEnemies()
        {
            foreach (var enemy in blockedEnemies)
            {
                enemy.OnReleased();
            }
            
            blockedEnemies.Clear();
        }
        
        // ブロック済みの敵をクリーンアップ
        public void CleanUpBlockedEnemies()
        {
            for (int i = blockedEnemies.Count - 1; i >= 0; i--)
            {
                if (blockedEnemies[i] == null || blockedEnemies[i].IsDead.CurrentValue)
                {
                    blockedEnemies.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 優先目標を取得する
        /// </summary>
        /// <param name="myPosition">自身の位置</param>
        /// <param name="enemiesInSight">攻撃対象リスト</param>
        /// <returns>優先目標</returns>
        public IEnemy GetBestTarget(Vector3 myPosition, IEnumerable<IEnemy> enemiesInSight)
        {
            var candidates = new List<IEnemy>();
            foreach (var e in enemiesInSight)
            {
                if (e != null && !e.IsDead.CurrentValue) 
                    candidates.Add(e);
            }

            if (candidates.Count == 0) 
                return null;

            switch (Stats.Priority)
            {
                case AttackPriority.FLYING_PRIORITIZED:
                    var flying = candidates.FindAll(e => e.IsFlying);
                    if (flying.Count > 0) return GetClosestEnemy(myPosition, flying);
                    return GetClosestEnemy(myPosition, candidates);

                case AttackPriority.BLOCK_ONLY:
                    var blocked = new List<IEnemy>();
                    foreach(var b in blockedEnemies) {
                        if(b != null && !b.IsDead.CurrentValue) 
                            blocked.Add(b);
                    }
                    
                    if (blocked.Count > 0) 
                        return GetClosestEnemy(myPosition, blocked);
                    
                    return null;

                case AttackPriority.CLOSEST:
                default:
                    return GetClosestEnemy(myPosition, candidates);
            }
        }

        /// <summary>
        /// 最短距離の敵を取得する
        /// </summary>
        /// <param name="myPosition">自身の位置</param>
        /// <param name="enemies">敵</param>
        /// <returns>最短距離にいる敵</returns>
        private IEnemy GetClosestEnemy(Vector3 myPosition, List<IEnemy> enemies)
        {
            if (enemies.Count == 0) 
                return null;

            IEnemy best = null;
            float minSqr = float.MaxValue;

            foreach (var e in enemies)
            {
                float sqr = (e.Position - myPosition).sqrMagnitude;
                if (sqr < minSqr)
                {
                    minSqr = sqr;
                    best = e;
                }
            }
            
            return best;
        }
    }
}



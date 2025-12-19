using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using RePuzzleKnights.Scripts.InGame.Allies.Enum;
using RePuzzleKnights.Scripts.InGame.Allies.Interface;
using RePuzzleKnights.Scripts.InGame.Allies.SO;
using RePuzzleKnights.Scripts.InGame.Enemies;
using RePuzzleKnights.Scripts.InGame.Enemies.Interface;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Allies
{
    /// <summary>
    /// 味方の戦闘ロジックを管理するコントローラークラス
    /// 攻撃、ブロック、死亡処理などのビジネスロジックを担当
    /// </summary>
    public class AllyBattleController : IDisposable, IAllyEntity
    {
        private readonly AllyBattleModel model;
        private readonly AllyDataSO allyData;
        private readonly Transform transform;

        private Action<AllyDataSO> onDeathCallback;
        private CompositeDisposable disposables = new();

        public Vector3 Position => transform.position;
        public bool IsDead => model.IsDead.CurrentValue;
        public AllyDataSO Data => allyData;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AllyBattleController(AllyBattleModel model, AllyDataSO data, Transform transform)
        {
            this.model = model;
            this.allyData = data;
            this.transform = transform;
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize(Action<AllyDataSO> onDeath)
        {
            onDeathCallback = onDeath;
        }

        /// <summary>
        /// 毎フレーム更新
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (model.IsDead.CurrentValue)
                return;
            
            model.UpdateAttackTimer(deltaTime);
            model.CleanUpLists();

            var nearbyEnemies = ScanNearbyEnemies();
            UpdateSightList(nearbyEnemies);

            if (allyData.AllyType == AllyType.Ground)
            {
                HandleBlocking(nearbyEnemies);
            }

            if (model.IsAttackReady())
            {
                TryAttack();
            }
        }

        /// <summary>
        /// 周囲の敵をスキャンしてリストで返す
        /// </summary>
        private List<IEnemyEntity> ScanNearbyEnemies()
        {
            var colliders = Physics.OverlapSphere(transform.position, allyData.SearchRadius, model.EnemyLayerMask);
            var foundEnemies = new List<IEnemyEntity>();

            foreach (var col in colliders)
            {
                if (col.TryGetComponent<EnemyEntityHolder>(out var holder))
                {
                    var enemy = holder.Entity;
                    if (enemy != null && !enemy.IsDead)
                    {
                        foundEnemies.Add(enemy);
                    }
                }
            }
            
            return foundEnemies;
        }
        
        /// <summary>
        /// 攻撃対象リストの更新
        /// </summary>
        private void UpdateSightList(List<IEnemyEntity> nearbyEnemies)
        {
            var inGridEnemies = new List<IEnemyEntity>();
            var blockedList = model.BlockedEnemies.CurrentValue;

            foreach (var enemy in nearbyEnemies)
            {
                bool isBlockedByMe = blockedList.Contains(enemy);
                if (IsEnemyInGridRange(enemy) || isBlockedByMe)
                {
                    inGridEnemies.Add(enemy);
                }
            }
            
            model.SetEnemiesInSight(inGridEnemies);
        }

        private bool IsEnemyInGridRange(IEnemyEntity enemy)
        {
            Vector3 localPos = transform.InverseTransformPoint(enemy.Position);
            
            int gridX = Mathf.RoundToInt(localPos.x);
            int gridY = Mathf.RoundToInt(localPos.z);
            
            Vector2Int targetGrid = new Vector2Int(gridX, gridY);
            return allyData.AttackRangeGrids.Contains(targetGrid);
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(float damage)
        {
            model.TakeDamage(damage);
        }

        /// <summary>
        /// 死亡処理の購読
        /// </summary>
        public void SubscribeDeathEvent(Action onDead)
        {
            model.IsDead
                .Where(dead => dead)
                .Subscribe(_ => onDead?.Invoke())
                .AddTo(disposables);
        }

        /// <summary>
        /// 攻撃リクエストイベントを取得
        /// </summary>
        public Observable<IList<IEnemyEntity>> GetAttackRequestObservable()
        {
            return model.OnAttackRequested;
        }
        
        /// <summary>
        /// 攻撃キャンセルイベントを取得
        /// </summary>
        public Observable<Unit> GetAttackCancelObservable()
        {
            return model.OnAttackCancelled;
        }

        /// <summary>
        /// 死亡時の処理
        /// </summary>
        public void OnDead()
        {
            foreach (var enemy in model.BlockedEnemies.CurrentValue)
            {
                enemy.OnReleased();
            }
            
            onDeathCallback?.Invoke(allyData);
        }

        /// <summary>
        /// ブロック処理
        /// </summary>
        private void HandleBlocking(List<IEnemyEntity> nearbyEnemies)
        {
            if (!model.CanBlock())
                return;
    
            foreach (var enemy in nearbyEnemies)
            {
                if (!model.CanBlock())
                    break;

                if (model.BlockedEnemies.CurrentValue.Contains(enemy))
                    continue;

                if (enemy.IsFlying)
                    continue;
        
                Vector3 allyPos = transform.position;
                Vector3 enemyPos = enemy.Position;
                allyPos.y = 0;
                enemyPos.y = 0;
        
                float distance = Vector3.Distance(allyPos, enemyPos);
                if (distance < 0.5f)
                {
                    enemy.OnBlocked(this);
                    model.BlockEnemy(enemy);
                    
                    // ブロック後、敵の位置を調整
                    ArrangeBlockedEnemies();
                }
            }
        }

        /// <summary>
        /// ブロックされた敵の位置を配置
        /// 敵の侵入方向を基準に、味方の周囲に扇形に配置
        /// </summary>
        private void ArrangeBlockedEnemies()
        {
            var blockedList = model.BlockedEnemies.CurrentValue;
            if (blockedList.Count == 0)
                return;

            float radius = 0.8f; // 味方からの距離
            Vector3 allyPos = transform.position;

            for (int i = 0; i < blockedList.Count; i++)
            {
                var enemy = blockedList[i];
                if (enemy == null || enemy.IsDead)
                    continue;

                // 敵の現在位置から侵入方向を計算
                Vector3 enemyPos = enemy.Position;
                Vector3 directionFromAlly = (enemyPos - allyPos).normalized;
                directionFromAlly.y = 0; // Y軸は無視

                // 複数の敵がいる場合、侵入方向を基準に扇形に配置
                float baseAngle = Mathf.Atan2(directionFromAlly.z, directionFromAlly.x);
                
                // 敵の数に応じて扇の角度を調整（最大60度の範囲内に配置）
                float fanAngle = Mathf.Min(60f, 30f * (blockedList.Count - 1));
                float angleOffset = 0f;
                
                if (blockedList.Count > 1)
                {
                    // 中央からの角度オフセットを計算
                    angleOffset = Mathf.Lerp(-fanAngle / 2f, fanAngle / 2f, i / (float)(blockedList.Count - 1));
                }
                
                float finalAngle = baseAngle + angleOffset * Mathf.Deg2Rad;
                
                Vector3 offset = new Vector3(
                    Mathf.Cos(finalAngle) * radius,
                    0f,
                    Mathf.Sin(finalAngle) * radius
                );

                Vector3 targetPos = allyPos + offset;
                targetPos.y = enemy.Position.y; // Y座標は維持

                // スムーズに移動
                enemy.SetTargetPosition(targetPos, 0.3f);
            }
        }

        /// <summary>
        /// 攻撃を試行
        /// </summary>
        private void TryAttack()
        {
            List<IEnemyEntity> targets = new List<IEnemyEntity>();
            var primaryTarget = model.GetBestTarget(transform.position);
            
            // 主要ターゲットが存在しない、または死亡している場合は攻撃をキャンセル
            if (primaryTarget == null || primaryTarget.IsDead)
            {
                model.CancelAttack();
                return;
            }

            switch (allyData.RangeType)
            {
                case AttackRangeType.SINGLE_TARGET:
                    targets.Add(primaryTarget);
                    break;
                
                case AttackRangeType.SPLASH_AROUND_TARGET:
                    targets.Add(primaryTarget);
                    var splashEnemies = ScanSplashTargets(primaryTarget);
                    targets.AddRange(splashEnemies);
                    break;
                
                case AttackRangeType.FULL_RANGE_AREA:
                    var allEnemies = model.GetAllTargets();
                    if (allEnemies != null)
                    {
                        targets.AddRange(allEnemies);
                    }
                    break;
            }

            // すべてのターゲットが死亡している場合は攻撃をキャンセル
            if (targets.Count == 0 || targets.All(t => t == null || t.IsDead))
            {
                model.CancelAttack();
                return;
            }

            if (targets.Count > 0)
            {
                model.RequestAttack(targets, allyData.AttackPower);
                model.ResetAttackTimer();
            }
        }

        private List<IEnemyEntity> ScanSplashTargets(IEnemyEntity primaryTarget)
        {
            var splashTargets = new List<IEnemyEntity>();

            Vector3 center = primaryTarget.Position;
            float radius = allyData.SplashRadius;

            var colliders = Physics.OverlapSphere(center, radius, model.EnemyLayerMask);
            foreach (var col in colliders)
            {
                if (col.TryGetComponent<EnemyEntityHolder>(out var holder))
                {
                    var enemy = holder.Entity;
                    if (enemy != null && !enemy.IsDead && enemy != primaryTarget)
                    {
                        if (enemy.IsFlying && !allyData.CanAttackFlying)
                            continue;
                        
                        splashTargets.Add(enemy);
                    }
                }
            }

            return splashTargets;
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.Enum;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.Interface;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.Allies
{
    /// <summary>
    /// 味方の戦闘ロジックを管理するコントローラークラス
    /// 攻撃、ブロック、死亡処理などのビジネスロジックを担当
    /// </summary>
    public class AllyBattleController : IDisposable, IAllyEntity
    {
        private readonly AllyBattleModel _model;
        public AllyBattleModel Model => _model;

        private readonly AllyDataSO _allyData;
        private readonly Transform _transform;

        private Action<AllyDataSO> _onDeathCallback;
        private CompositeDisposable _disposables = new();

        public Vector3 Position => _transform.position;
        public Vector3 FacingDirection
        {
            get
            {
                float yAngle = _transform.eulerAngles.y;
                bool isFacingRight = Mathf.Abs(Mathf.DeltaAngle(yAngle, 90f)) < 45f;
                return isFacingRight ? Vector3.right : Vector3.left;
            }
        }
        public bool IsDead => _model.IsDead.CurrentValue;
        public AllyDataSO Data => _allyData;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AllyBattleController(AllyBattleModel model, AllyDataSO data, Transform transform)
        {
            this._model = model;
            this._allyData = data;
            this._transform = transform;
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize(Action<AllyDataSO> onDeath)
        {
            _onDeathCallback = onDeath;
        }

        /// <summary>
        /// 毎フレーム更新
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (_model.IsDead.CurrentValue)
                return;
            
            int beforeBlockedCount = _model.BlockedEnemies.CurrentValue.Count;
            _model.UpdateAttackTimer(deltaTime);
            _model.CleanUpLists();
            int afterBlockedCount = _model.BlockedEnemies.CurrentValue.Count;

            // 敵が死ぬなどして数が減った場合、再配置する
            if (beforeBlockedCount != afterBlockedCount && afterBlockedCount > 0)
            {
                ArrangeBlockedEnemies();
            }

            var nearbyEnemies = ScanNearbyEnemies();
            UpdateSightList(nearbyEnemies);

            if (_allyData.AllyType == AllyType.Ground)
            {
                HandleBlocking(nearbyEnemies);
            }

            if (_model.IsAttackReady())
            {
                TryAttack();
            }
        }

        /// <summary>
        /// 周囲の敵をスキャンしてリストで返す
        /// </summary>
        private List<IEnemyEntity> ScanNearbyEnemies()
        {
            var colliders = Physics.OverlapSphere(_transform.position, _allyData.SearchRadius, _model.EnemyLayerMask);
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
            var blockedList = _model.BlockedEnemies.CurrentValue;

            foreach (var enemy in nearbyEnemies)
            {
                bool isBlockedByMe = blockedList.Contains(enemy);
                if (IsEnemyInGridRange(enemy) || isBlockedByMe)
                {
                    inGridEnemies.Add(enemy);
                }
            }
            
            _model.SetEnemiesInSight(inGridEnemies);
        }

        private bool IsEnemyInGridRange(IEnemyEntity enemy)
        {
            Vector3 localPos = _transform.InverseTransformPoint(enemy.Position);
            
            int gridX = Mathf.RoundToInt(localPos.x);
            int gridY = Mathf.RoundToInt(localPos.z);
            
            Vector2Int targetGrid = new Vector2Int(gridX, gridY);
            return _allyData.AttackRangeGrids.Contains(targetGrid);
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(float damage)
        {
            _model.TakeDamage(damage);
        }

        /// <summary>
        /// 死亡処理の購読
        /// </summary>
        public void SubscribeDeathEvent(Action onDead)
        {
            _model.IsDead
                .Where(dead => dead)
                .Subscribe(_ => onDead?.Invoke())
                .AddTo(_disposables);
        }

        /// <summary>
        /// 攻撃リクエストイベントを取得
        /// </summary>
        public Observable<IList<IEnemyEntity>> GetAttackRequestObservable()
        {
            return _model.OnAttackRequested;
        }
        
        /// <summary>
        /// 攻撃キャンセルイベントを取得
        /// </summary>
        public Observable<Unit> GetAttackCancelObservable()
        {
            return _model.OnAttackCancelled;
        }

        /// <summary>
        /// 死亡時の処理
        /// </summary>
        public void OnDead()
        {
            foreach (var enemy in _model.BlockedEnemies.CurrentValue)
            {
                enemy.OnReleased();
            }
            
            _onDeathCallback?.Invoke(_allyData);
        }

        /// <summary>
        /// ブロック処理
        /// </summary>
        private void HandleBlocking(List<IEnemyEntity> nearbyEnemies)
        {
            if (!_model.CanBlock())
                return;
    
            foreach (var enemy in nearbyEnemies)
            {
                if (!_model.CanBlock())
                    break;

                if (_model.BlockedEnemies.CurrentValue.Contains(enemy))
                    continue;

                if (enemy.IsFlying)
                    continue;
        
                Vector3 allyPos = _transform.position;
                Vector3 enemyPos = enemy.Position;
                allyPos.y = 0;
                enemyPos.y = 0;
        
                float distance = Vector3.Distance(allyPos, enemyPos);
                if (distance < 0.5f)
                {
                    enemy.OnBlocked(this);
                    _model.BlockEnemy(enemy);
                    
                    // ブロック後、敵の位置を調整
                    ArrangeBlockedEnemies();
                }
            }
        }

        /// <summary>
        /// ブロックされた敵の位置を配置
        /// 味方の正面方向を基準に、周囲に扇形に配置
        /// </summary>
        private void ArrangeBlockedEnemies()
        {
            var blockedList = _model.BlockedEnemies.CurrentValue;
            if (blockedList.Count == 0)
                return;

            float radius = 0.8f; // 味方からの距離
            Vector3 allyPos = _transform.position;
            
            // 味方の正面方向を基準角とする
            Vector3 faceDir = FacingDirection;
            float baseAngle = Mathf.Atan2(faceDir.z, faceDir.x);

            for (int i = 0; i < blockedList.Count; i++)
            {
                var enemy = blockedList[i];
                if (enemy == null || enemy.IsDead)
                    continue;

                // 敵の数に応じて扇の角度を調整
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
            var primaryTarget = _model.GetBestTarget(_transform.position);
            
            // 主要ターゲットが存在しない、または死亡している場合は攻撃をキャンセル
            if (primaryTarget == null || primaryTarget.IsDead)
            {
                _model.CancelAttack();
                return;
            }

            switch (_allyData.RangeType)
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
                    var allEnemies = _model.GetAllTargets();
                    if (allEnemies != null)
                    {
                        targets.AddRange(allEnemies);
                    }
                    break;
            }

            // すべてのターゲットが死亡している場合は攻撃をキャンセル
            if (targets.Count == 0 || targets.All(t => t == null || t.IsDead))
            {
                _model.CancelAttack();
                return;
            }

            if (targets.Count > 0)
            {
                _model.RequestAttack(targets, _allyData.AttackPower);
                _model.ResetAttackTimer();
            }
        }

        private List<IEnemyEntity> ScanSplashTargets(IEnemyEntity primaryTarget)
        {
            var splashTargets = new List<IEnemyEntity>();

            Vector3 center = primaryTarget.Position;
            float radius = _allyData.SplashRadius;

            var colliders = Physics.OverlapSphere(center, radius, _model.EnemyLayerMask);
            foreach (var col in colliders)
            {
                if (col.TryGetComponent<EnemyEntityHolder>(out var holder))
                {
                    var enemy = holder.Entity;
                    if (enemy != null && !enemy.IsDead && enemy != primaryTarget)
                    {
                        if (enemy.IsFlying && !_allyData.CanAttackFlying)
                            continue;
                        
                        splashTargets.Add(enemy);
                    }
                }
            }

            return splashTargets;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}





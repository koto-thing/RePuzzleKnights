using System.Collections.Generic;
using R3;
using RePuzzleKnights.Scripts.InGame.Allies.Interface;
using RePuzzleKnights.Scripts.InGame.Enemies.SO;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    /// <summary>
    /// 敵キャラクターのデータと状態を管理するModelクラス
    /// ステータス、移動、状態、ブロック情報などを一元管理
    /// </summary>
    public class EnemyModel
    {
        // 敵データ
        public EnemyDataSO Data { get; }
        
        // ステータス管理
        public EnemyStatus Status { get; } = new();
        
        // 移動制御
        public EnemyMover Mover { get; } = new();
        
        // 状態管理（ステルス、スタン等）
        public EnemyStateController StateManager { get; } = new();

        // 攻撃タイマー
        public ReadOnlyReactiveProperty<float> AttackTimer => attackTimer;
        private readonly ReactiveProperty<float> attackTimer = new(0.0f);

        // 現在ブロックしている味方のリスト
        public ReadOnlyReactiveProperty<List<IAllyEntity>> BlockedBy => blockedBy;
        private readonly ReactiveProperty<List<IAllyEntity>> blockedBy = new(new List<IAllyEntity>());

        // 現在ブロックしている味方（最初の1体）
        public ReadOnlyReactiveProperty<IAllyEntity> CurrentBlocker => currentBlocker;
        private readonly ReactiveProperty<IAllyEntity> currentBlocker = new();
        
        // 移動可能かどうか
        public ReadOnlyReactiveProperty<bool> CanMove => canMove;
        private readonly ReactiveProperty<bool> canMove = new(true);

        // 目標位置（ブロック時の位置調整用）
        public Observable<(Vector3 position, float duration)> OnTargetPositionChanged => onTargetPositionChanged;
        private readonly Subject<(Vector3 position, float duration)> onTargetPositionChanged = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EnemyModel(EnemyDataSO data, List<Vector3> path)
        {
            Data = data;
            Status.Initialize(data);
            StateManager.Initialize(data.InitialStates);
            Mover.Initialize(data, path);
        }

        /// <summary>
        /// 攻撃タイマーを更新
        /// </summary>
        public void UpdateAttackTimer(float deltaTime)
        {
            attackTimer.Value += deltaTime;
        }

        /// <summary>
        /// 攻撃タイマーをリセット
        /// </summary>
        public void ResetAttackTimer()
        {
            attackTimer.Value = 0.0f;
        }

        /// <summary>
        /// 攻撃可能かどうか判定
        /// </summary>
        public bool CanAttack()
        {
            return attackTimer.Value >= Data.AttackInterval;
        }

        /// <summary>
        /// ブロックされた際の処理
        /// </summary>
        public void SetBlocker(IAllyEntity blocker)
        {
            if (blocker == null)
                return;

            if (!blockedBy.Value.Contains(blocker))
            {
                var newList = new List<IAllyEntity>(blockedBy.Value) { blocker };
                blockedBy.Value = newList;
                currentBlocker.Value = blocker; // 最初のブロッカーを設定
                canMove.Value = false; // 移動停止
                Mover.SetBlocked(true);
            }
        }

        /// <summary>
        /// ブロックから解放
        /// </summary>
        public void ClearBlocker()
        {
            blockedBy.Value = new List<IAllyEntity>();
            currentBlocker.Value = null;
            canMove.Value = true; // 移動再開
            Mover.SetBlocked(false);
        }

        /// <summary>
        /// 目標位置を設定（スムーズな移動用）
        /// </summary>
        public void SetTargetPosition(Vector3 position, float duration)
        {
            onTargetPositionChanged.OnNext((position, duration));
        }
    }
}

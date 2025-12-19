using System;
using R3;
using RePuzzleKnights.Scripts.InGame.Allies.Interface;
using RePuzzleKnights.Scripts.InGame.Enemies.Interface;
using RePuzzleKnights.Scripts.InGame.Enemies.SO;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    /// <summary>
    /// 敵キャラクターの制御を行うコントローラークラス
    /// ビジネスロジックに集中し、ViewやModelとの連携を管理
    /// </summary>
    public class EnemyController : IDisposable, IEnemyEntity
    {
        private readonly EnemyModel model;
        private readonly Transform transform;
        
        private readonly CompositeDisposable disposables = new();

        // IEnemyEntity実装
        public bool IsFlying => model.Data.MoveType == MovementType.FLYING;
        public bool IsDead => model.Status.IsDead.CurrentValue;
        public Vector3 Position => transform.position;
        public float DistanceToGoal => Vector3.Distance(Position, model.Mover.CurrentTarget.CurrentValue);

        // Model参照の公開（Presenterで使用）
        public EnemyModel Model => model;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EnemyController(EnemyModel model, Transform transform)
        {
            this.model = model;
            this.transform = transform;
        }

        /// <summary>
        /// 毎フレーム更新
        /// </summary>
        public void Tick(float deltaTime)
        {
            var blocker = model.CurrentBlocker.CurrentValue;
            
            if (blocker != null)
            {
                // ブロッカーが死亡していたら解放
                if (blocker.IsDead)
                {
                    OnReleased();
                }
                else
                {
                    // 攻撃処理
                    model.UpdateAttackTimer(deltaTime);
                    if (model.CanAttack())
                    {
                        AttackBlocker(blocker);
                    }
                }
            }
        }

        /// <summary>
        /// ブロックされた際の処理
        /// </summary>
        public void OnBlocked(IAllyEntity blocker)
        {
            if (model.StateManager.HasState(EnemyState.UNSTOPPABLE))
                return;

            model.SetBlocker(blocker);
        }

        /// <summary>
        /// ブロックから解放された際の処理
        /// </summary>
        public void OnReleased()
        {
            model.ClearBlocker();
        }

        /// <summary>
        /// ダメージを受ける処理
        /// </summary>
        public void TakeDamage(float damage)
        {
            model.Status.TakeDamage(damage);
        }
        
        /// <summary>
        /// ステルス状態かどうかを判定
        /// </summary>
        public bool IsStealth() => model.StateManager.HasState(EnemyState.STEALTH);

        /// <summary>
        /// 位置を設定（スムーズな移動用）
        /// </summary>
        public void SetTargetPosition(Vector3 position, float duration = 0.3f)
        {
            model.SetTargetPosition(position, duration);
        }

        /// <summary>
        /// ブロッカーを攻撃
        /// </summary>
        private void AttackBlocker(IAllyEntity blocker)
        {
            blocker.TakeDamage(model.Data.AttackPower);
            model.ResetAttackTimer();
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
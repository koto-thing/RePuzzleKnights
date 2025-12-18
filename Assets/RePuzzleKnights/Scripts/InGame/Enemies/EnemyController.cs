using System;
using System.Collections.Generic;
using R3;
using RePuzzleKnights.Scripts.InGame.BaseSystem;
using RePuzzleKnights.Scripts.InGame.Enemies.Interface;
using UnityEngine;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    /// <summary>
    /// 敵キャラクターの制御を行うコントローラークラス
    /// ステータス、移動、状態管理を統合して管理する
    /// </summary>
    public class EnemyController : IInitializable, ITickable, IDisposable, IEnemyEntity
    {
        // ステータス管理
        public EnemyStatus Status { get; } = new();
        
        // 移動制御
        public EnemyMover Mover { get; } = new();
        
        // 状態管理（ステルス、スタン等）
        public EnemyStateController StateManager { get; } = new();
        
        private readonly EnemyView view;
        private readonly EnemyDataSO data;
        private readonly BaseStatusModel baseStatusModel;
        
        private readonly CompositeDisposable disposables = new();

        // 飛行タイプかどうか
        public bool IsFlying => data.MoveType == MovementType.FLYING;
        
        // 死亡しているか
        public bool IsDead => Status.IsDead.CurrentValue;
        
        // 現在位置
        public Vector3 Position => view.transform.position;
        
        // ゴールまでの距離
        // ゴールまでの距離
        public float DistanceToGoal => Vector3.Distance(Position, Mover.CurrentTarget.CurrentValue);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EnemyController(
            EnemyView view, 
            EnemyDataSO data, 
            BaseStatusModel baseStatusModel, 
            List<Vector3> path)
        {
            this.view = view;
            this.data = data;
            this.baseStatusModel = baseStatusModel;

            Status.Initialize(data);
            StateManager.Initialize(data.InitialStates);
            Mover.Initialize(data, path);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize()
        {
            view.SetController(this);
            view.InitializeStatusDisplay(Status, data.MaxHp);
            SubscribeEvents();
        }

        public void Tick()
        {
            // 必要なら毎フレーム処理
        }

        /// <summary>
        /// イベント購読の設定
        /// </summary>
        private void SubscribeEvents()
        {
            // 移動
            Mover.CurrentTarget
                .Subscribe(target =>
                {
                    view.MoveTo(target, data.MoveSpeed, () => Mover.OnArrivedAtTarget());
                })
                .AddTo(disposables);

            // 死亡
            Status.IsDead
                .Where(dead => dead)
                .Subscribe(_ =>
                {
                    view.DestroyActor();
                    Dispose();
                })
                .AddTo(disposables);

            // ゴール到達
            Mover.OnGoalReached
                .Subscribe(_ =>
                {
                    // 本拠地にダメージ
                    baseStatusModel.TakeDamage(1);
                    
                    view.DestroyActor();
                    Dispose();
                })
                .AddTo(disposables);
        }

        /// <summary>
        /// 敵がブロックされた際の処理
        /// </summary>
        public void OnBlocked(MonoBehaviour blocker)
        {
            if (StateManager.HasState(EnemyState.UNSTOPPABLE))
                return;
            
            Mover.SetBlocked(true);
            view.PauseMove();
        }

        /// <summary>
        /// ブロックから解放された際の処理
        /// </summary>
        public void OnReleased()
        {
            Mover.SetBlocked(false);
            view.ResumeMove();
        }

        /// <summary>
        /// ダメージを受ける処理
        /// </summary>
        public void TakeDamage(float damage)
        {
            Status.TakeDamage(damage);
            view.PlayDamageEffect();
        }
        
        /// <summary>
        /// ステルス状態かどうかを判定
        /// </summary>
        public bool IsStealth() => StateManager.HasState(EnemyState.STEALTH);

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
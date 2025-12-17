using System;
using System.Collections.Generic;
using R3;
using RePuzzleKnights.Scripts.InGame.BaseSystem;
using RePuzzleKnights.Scripts.InGame.Enemies.Interface;
using UnityEngine;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.InGame.Enemies.BugEnemy
{
    public class BugEnemyMoveController : IInitializable, ITickable, IDisposable, IEnemyEntity
    {
        private BugEnemyMoveModel model;
        private BugEnemyStatusModel statusModel;
        private BugEnemyView view;
        private BugEnemyDataSO enemyData;
        private BaseStatusModel baseStatusModel;
        
        public bool IsFlying => enemyData.IsFlying;
        public bool IsDead => statusModel.IsDead.CurrentValue;
        public Vector3 Position => view.transform.position;
        public float DistanceToGoal => Vector3.Distance(Position, model.CurrentTarget.CurrentValue);
        
        private CompositeDisposable disposables = new ();

        public BugEnemyMoveController(BugEnemyMoveModel model, BugEnemyStatusModel statusModel, BugEnemyView view, BugEnemyDataSO data, BaseStatusModel baseStatusModel, List<Vector3> wayPoints)
        {
            this.model = model;
            this.statusModel = statusModel;
            this.view = view;
            this.enemyData = data;
            this.baseStatusModel = baseStatusModel;

            model.Initialize(wayPoints);
            statusModel.Initialize(data.MaxHp);
        }
        
        public void Initialize()
        {
            SubscribeEvents();
        }

        public void Tick()
        {
            if (statusModel.AttackCooldownTimer > 0)
                statusModel.AttackCooldownTimer -= Time.deltaTime;
        }

        private void SubscribeEvents()
        {
            model.CurrentTarget
                .Subscribe(target =>
                {
                    view.MoveTo(target, enemyData.MoveSpeed, () => 
                    {
                        if (!statusModel.IsBlocked.CurrentValue)
                        {
                            model.NotifyReachedTarget();
                        }
                    });
                })
                .AddTo(disposables);
            
            model.OnGoalReached
                .Subscribe(_ =>
                {
                    baseStatusModel.TakeDamage(1);
                    view.DestroyActor();
                    Dispose();
                })
                .AddTo(disposables);
            
            statusModel.IsBlocked
                .Subscribe(blocked =>
                {
                    if (blocked)
                        view.PauseMove();
                    else
                        view.ResumeMove();
                })
                .AddTo(disposables);
            
            statusModel.IsDead
                .Where(dead => dead)
                .Subscribe(_ =>
                {
                    view.DestroyActor();
                    Dispose();
                })
                .AddTo(disposables);
        }
        
        public void OnBlocked(MonoBehaviour blocker)
        {
            Debug.Log($"[BugEnemyMoveController] Enemy blocked by {blocker.name}");
            statusModel.SetBlocked(true);
        }

        public void OnReleased()
        {
            Debug.Log($"[BugEnemyMoveController] Enemy released");
            statusModel.SetBlocked(false);
        }

        public void TakeDamage(float damage)
        {
            Debug.Log($"[BugEnemyMoveController] Enemy took {damage} damage. Current HP: {statusModel.CurrentHp.CurrentValue}");
            statusModel.TakeDamage(damage);
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
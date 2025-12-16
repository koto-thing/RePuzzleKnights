using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.InGame.Enemies.BugEnemy
{
    public class BugEnemyMoveController : IInitializable, IDisposable
    {
        private BugEnemyMoveModel model;
        private BugEnemyView view;
        private BugEnemyDataSO enemyData;

        private readonly List<Vector3> initialWayPoints;
        private CompositeDisposable disposables = new ();

        public BugEnemyMoveController(BugEnemyMoveModel model, BugEnemyView view, BugEnemyDataSO data, List<Vector3> wayPoints)
        {
            this.model = model;
            this.view = view;
            this.enemyData = data;
            this.initialWayPoints = wayPoints;
        }
        
        public void Initialize()
        {
            SubscribeEvents();
            
            model.StartMove(initialWayPoints);
        }

        private void SubscribeEvents()
        {
            model.OnChangeTarget
                .Subscribe(target =>
                {
                    view.MoveTo(target, enemyData.MoveSpeed, () => model.NotifyReachedTarget());
                })
                .AddTo(disposables);

            model.OnMoveToNext
                .Subscribe(target =>
                {
                    view.MoveTo(target, enemyData.MoveSpeed, () => model.NotifyReachedTarget());
                })
                .AddTo(disposables);
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
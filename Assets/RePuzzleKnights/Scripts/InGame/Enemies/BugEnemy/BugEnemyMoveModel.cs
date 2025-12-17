using System.Collections.Generic;
using R3;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Enemies.BugEnemy
{
    public class BugEnemyMoveModel
    {
        private int currentWayPointIndex = 0;
        private List<Vector3> wayPoints = new();

        public ReadOnlyReactiveProperty<Vector3> CurrentTarget => currentTarget;
        private readonly ReactiveProperty<Vector3> currentTarget = new();
        
        public Observable<Unit> OnGoalReached => onGoalReached;
        private readonly Subject<Unit> onGoalReached = new();

        public void Initialize(List<Vector3> path)
        {
            this.wayPoints = path;
            this.currentWayPointIndex = 0;
            
            if (wayPoints != null && wayPoints.Count > 0)
            {
                currentTarget.Value = wayPoints[0];
            }
        }

        public void NotifyReachedTarget()
        {
            currentWayPointIndex++;
            if (currentWayPointIndex < wayPoints.Count)
            {
                currentTarget.Value = wayPoints[currentWayPointIndex];
            }
            else
            {
                onGoalReached.OnNext(Unit.Default);
            }
        }
    }
}
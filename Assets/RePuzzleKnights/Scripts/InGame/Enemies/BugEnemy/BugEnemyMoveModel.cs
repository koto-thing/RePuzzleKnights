using System.Collections.Generic;
using R3;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Enemies.BugEnemy
{
    public class BugEnemyMoveModel
    {
        private int currentWayPointIndex = 0;
        private List<Vector3> wayPoints = new ();
        
        private ReactiveProperty<Vector3> currentTarget = new();
        private Subject<Vector3> onMoveToNext = new();

        public Observable<Vector3> OnChangeTarget => currentTarget;
        public Observable<Vector3> OnMoveToNext => onMoveToNext;

        /// <summary>
        /// Initializes the movement of the enemy by setting the waypoints it should follow and starting at the first waypoint.
        /// </summary>
        /// <param name="newWayPoints">A list of waypoints representing the path that the enemy will follow.</param>
        public void StartMove(List<Vector3> newWayPoints)
        {
            if (newWayPoints == null || newWayPoints.Count == 0)
                return;

            wayPoints = newWayPoints;
            
            currentWayPointIndex = 0;
            currentTarget.Value = wayPoints[currentWayPointIndex];
        }

        /// <summary>
        /// Updates the current target to the next waypoint when the enemy reaches its current target.
        /// If there are more waypoints remaining, it notifies subscribers about the change
        /// by publishing the next waypoint through the reactive properties.
        /// </summary>
        public void NotifyReachedTarget()
        {
            currentWayPointIndex++;
            if (currentWayPointIndex < wayPoints.Count)
            {
                currentTarget.Value = wayPoints[currentWayPointIndex];
                onMoveToNext.OnNext(currentTarget.CurrentValue);
            }
        }
    }
}
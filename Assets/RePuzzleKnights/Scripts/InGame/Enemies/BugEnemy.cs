using System.Collections.Generic;
using UnityEngine;
using RePuzzleKnights.Scripts.InGame.Enemies.Interface;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    public class BugEnemy : IGroundEnemy
    {
        private float moveSpeed = 1.0f;
        private List<Vector3> wayPoints = new ();

        public float MoveSpeed => moveSpeed;
        public List<Vector3> WayPoints => wayPoints;

        public void MoveAlongPath()
        {
            
        }
    }
}
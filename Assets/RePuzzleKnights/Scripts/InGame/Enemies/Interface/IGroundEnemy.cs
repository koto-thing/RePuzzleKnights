using System.Collections.Generic;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Enemies.Interface
{
    public interface IGroundEnemy
    {
        public float MoveSpeed { get; }
        public List<Vector3> WayPoints { get; }

        public void MoveAlongPath();
    }
}
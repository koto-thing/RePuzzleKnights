using System.Collections.Generic;
using RePuzzleKnights.Scripts.InGame.Enemies.Interface;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Enemies.BugEnemy
{
    [CreateAssetMenu(fileName = "New BugEnemyData", menuName = "Enemies/Create New BugEnemyData")]
    public class BugEnemyDataSO : ScriptableObject, IGroundEnemy
    {
        [SerializeField, Tooltip("HP")] private float hp = 100.0f;
        [SerializeField, Tooltip("防御力")] private float defense = 10.0f;
        [SerializeField, Tooltip("攻撃力")] private float attack = 10.0f;
        [SerializeField, Tooltip("移動速度")] private float moveSpeed = 1.0f;
        [SerializeField, Tooltip("移動経路(手動)")] private List<Vector3> wayPoints = new ();

        public float Hp => hp;
        public float Defense => defense;
        public float Attack => attack;
        public float MoveSpeed => moveSpeed;
        public List<Vector3> WayPoints => wayPoints;
    }
}
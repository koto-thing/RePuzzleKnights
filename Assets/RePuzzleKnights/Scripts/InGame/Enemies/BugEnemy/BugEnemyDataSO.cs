using System.Collections.Generic;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Enemies.BugEnemy
{
    [CreateAssetMenu(fileName = "New BugEnemyData", menuName = "Enemies/Create New BugEnemyData")]
    public class BugEnemyDataSO : ScriptableObject
    {
        [Header("基本設定")] public float MaxHp = 100.0f;
        public float MoveSpeed = 1.0f;
        public bool IsFlying = false;
        
        [Header("戦闘設定")]
        [Tooltip("攻撃力")] public float AttackPower = 10.0f;
        [Tooltip("攻撃間隔")] public float AttackInterval = 1.5f;
        [Tooltip("攻撃射程")] public float AttckRange = 0.5f;
        
        [Header("移動設定")]
        [Tooltip("移動経路(手動)")] public List<Vector3> WayPoints = new ();
    }
}
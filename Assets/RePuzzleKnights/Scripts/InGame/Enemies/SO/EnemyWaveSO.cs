using System.Collections.Generic;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Enemies.SO
{
    /// <summary>
    /// 敵のウェーブ情報を定義するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New EnemyWaveSO", menuName = "Enemies/Create New EnemyWaveSO")]
    public class EnemyWaveSO : ScriptableObject
    {
        [Tooltip("このウェーブ出現させる敵のリスト")] 
        public List<EnemySpawnEntry> SpawnEntries = new();
    }
}
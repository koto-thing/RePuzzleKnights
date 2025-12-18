using System;
using System.Collections.Generic;
using RePuzzleKnights.Scripts.InGame.Enemies;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    /// <summary>
    /// 敵の生成情報を定義するデータクラス
    /// 生成する敵の種類、数、間隔等を設定
    /// </summary>
    [Serializable]
    public class EnemySpawnEntry
    {
        [Tooltip("生成する敵のデータ")] 
        public EnemyDataSO EnemyDataSO;
        
        [Tooltip("生成する敵のプレハブ")] 
        public AssetReferenceGameObject EnemyPrefabRef;
        
        [Tooltip("生成数")] 
        public int Count = 1;
        
        [Tooltip("生成間隔(秒)")] 
        public float Interval = 1.0f;
        
        [Tooltip("ウェーブ開始からの待機時間(秒)")] 
        public float InitialDelay = 0.0f;
    }

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
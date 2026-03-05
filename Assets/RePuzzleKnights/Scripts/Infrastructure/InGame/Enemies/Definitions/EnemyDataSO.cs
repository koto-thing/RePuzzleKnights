using System.Collections.Generic;
using RePuzzleKnights.Scripts.Domain.Enums;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.Enemies.Definitions
{
    /// <summary>
    /// 敵キャラクターのデータを定義するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New EnemyData", menuName = "Enemies/Create New Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("基本情報")]
        public string EnemyName;
        public EnemyRank Rank;
        public AssetReferenceGameObject PrefabRef;

        [Header("ステータス")]
        public float MaxHp = 100f;
        public float MoveSpeed = 1.0f;
        
        [Header("移動設定")]
        public MovementType MoveType;
        [Tooltip("飛行時に地形を無視するか")] public bool IgnoreTerrain = false;

        [Header("攻撃設定")]
        public float AttackPower = 10.0f;
        public float AttackInterval = 1.5f;
        public float AttackRange = 0.5f;
        public AttackType AttackType;
        public AttackAreaType AreaType;
        [Tooltip("範囲攻撃の場合の半径")] public float ExplosionRadius = 0.0f;

        [Header("耐性・特性")]
        [Tooltip("初期状態（ステルスなど）")] public List<EnemyState> InitialStates;
    }
}


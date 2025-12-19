using System.Collections.Generic;
using RePuzzleKnights.Scripts.InGame.Allies.Enum;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RePuzzleKnights.Scripts.InGame.Allies.SO
{
    public enum AttackRangeType
    {
        SINGLE_TARGET,
        SPLASH_AROUND_TARGET,
        FULL_RANGE_AREA,
    }

    public enum AttackPriority
    {
        CLOSEST,
        FLYING_PRIORITIZED,
        BLOCK_ONLY
    }
    
    [CreateAssetMenu(fileName = "New AllyData", menuName = "Allies/Create New AllyData")]
    public class AllyDataSO : ScriptableObject
    {
        [Header("基本設定")] 
        public string AllyName;
        public AllyType AllyType;
        public AssetReferenceGameObject PrefabRef;

        [Header("戦闘ステータス")] 
        public float MaxHp = 100.0f;
        public float AttackPower = 10.0f;
        public float AttackRange = 3.0f;
        public float AttackInterval = 1.0f;
        public int BlockCount = 1;

        [Header("攻撃範囲(ローカル座標: x横, y奥)")] 
        public List<Vector2Int> AttackRangeGrids = new List<Vector2Int>
        {
            new (0, 1),
        };

        [Header("索敵設定")] 
        public float SearchRadius;

        [Header("攻撃設定")] 
        public AttackRangeType RangeType;
        public AttackPriority Priority;

        [Tooltip("範囲攻撃の場合の攻撃半径")] 
        public float SplashRadius = 1.5f;

        [Tooltip("飛行敵を攻撃できるか")] 
        public bool CanAttackFlying = false;
    }
}
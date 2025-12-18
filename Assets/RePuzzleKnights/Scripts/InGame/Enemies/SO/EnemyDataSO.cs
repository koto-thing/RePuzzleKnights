using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    /// <summary>
    /// 敵のランク（強さ）を定義
    /// </summary>
    public enum EnemyRank
    {
        NORMAL,  // 通常
        ELITE,   // エリート
        BOSS,    // ボス
    }

    /// <summary>
    /// 敵の移動タイプを定義
    /// </summary>
    public enum MovementType
    {
        GROUND,  // 地上移動
        FLYING,  // 飛行
    }

    /// <summary>
    /// 敵の攻撃タイプを定義
    /// </summary>
    public enum AttackType
    {
        MELEE,   // 近接攻撃
        RANGED,  // 遠距離攻撃
    }

    /// <summary>
    /// 敵の攻撃範囲タイプを定義
    /// </summary>
    public enum AttackAreaType
    {
        SINGLE,  // 単体攻撃
        AREA,    // 範囲攻撃
    }

    /// <summary>
    /// 敵の特殊状態を定義
    /// </summary>
    public enum EnemyState
    {
        STEALTH,      // ステルス（不可視）
        STUNNED,      // スタン（行動不能）
        UNSTOPPABLE,  // 停止不可（ブロック無効）
    }
    
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
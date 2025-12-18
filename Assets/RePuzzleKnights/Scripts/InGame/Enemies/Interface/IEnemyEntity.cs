using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Enemies.Interface
{
    /// <summary>
    /// 敵エンティティの基本インターフェース
    /// 外部から敵を操作する際に使用
    /// </summary>
    public interface IEnemyEntity
    {
        // 飛行タイプかどうか
        bool IsFlying { get; }
        
        // 死亡しているか
        bool IsDead { get; }
        
        // 現在位置
        Vector3 Position { get; }
        
        // ゴールまでの距離
        float DistanceToGoal { get; }

        // ブロックされた際の処理
        void OnBlocked(MonoBehaviour blocker);
        
        // ブロックから解放された際の処理
        void OnReleased();
        
        // ダメージを受ける処理
        void TakeDamage(float damage);
    }
}
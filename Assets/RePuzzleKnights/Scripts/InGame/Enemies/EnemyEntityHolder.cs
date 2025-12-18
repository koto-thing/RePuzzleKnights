using RePuzzleKnights.Scripts.InGame.Enemies.Interface;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    /// <summary>
    /// 敵エンティティへの参照を保持するホルダークラス
    /// Colliderがアタッチされたオブジェクトに設置し、外部（味方の攻撃判定など）からアクセスできるようにする
    /// </summary>
    public class EnemyEntityHolder : MonoBehaviour
    {
        public IEnemyEntity Entity { get; private set; }

        /// <summary>
        /// Controller生成時に呼ばれ、エンティティをセットする
        /// </summary>
        public void Initialize(IEnemyEntity entity)
        {
            this.Entity = entity;
        }
    }
}
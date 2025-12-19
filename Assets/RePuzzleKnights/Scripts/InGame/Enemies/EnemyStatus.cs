using R3;
using RePuzzleKnights.Scripts.InGame.Enemies.SO;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    /// <summary>
    /// 敵のステータス（HP、ランク等）を管理するクラス
    /// </summary>
    public class EnemyStatus
    {
        // 現在のHP
        public ReadOnlyReactiveProperty<float> CurrentHp => currentHp;
        private readonly ReactiveProperty<float> currentHp = new();

        // 死亡フラグ
        public ReadOnlyReactiveProperty<bool> IsDead => isDead;
        private readonly ReactiveProperty<bool> isDead = new(false);

        // 敵のランク（通常、エリート、ボス等）
        public EnemyRank Rank { get; private set; }

        /// <summary>
        /// ステータスの初期化
        /// </summary>
        public void Initialize(EnemyDataSO data)
        {
            currentHp.Value = data.MaxHp;
            isDead.Value = false;
            Rank = data.Rank;
        }

        /// <summary>
        /// ダメージを受ける処理
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (isDead.Value) 
                return;
            
            currentHp.Value -= damage;
            if (currentHp.Value <= 0) 
                isDead.Value = true;
        }
    }
}
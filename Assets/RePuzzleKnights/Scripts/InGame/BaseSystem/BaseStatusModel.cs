using R3;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.BaseSystem
{
    /// <summary>
    /// 本拠地のステータスを管理するModelクラス
    /// 耐久値とその変化を管理
    /// </summary>
    public class BaseStatusModel
    {
        private const int maxDurability = 10;
        public int MaxDurability => maxDurability;

        public ReadOnlyReactiveProperty<int> CurrentDurability => currentDurability;
        private readonly ReactiveProperty<int> currentDurability;

        public Observable<Unit> OnBaseDestroyed => onBaseDestroyed;
        private readonly Subject<Unit> onBaseDestroyed = new();
        
        public BaseStatusModel()
        {
            currentDurability = new ReactiveProperty<int>(maxDurability);
        }

        /// <summary>
        /// ダメージを受ける処理
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (damage <= 0 || currentDurability.Value <= 0)
                return;
            
            int nextValue = Mathf.Max(0, currentDurability.Value - damage);
            currentDurability.Value = nextValue;

            if (currentDurability.Value <= 0)
            {
                onBaseDestroyed.OnNext(Unit.Default);
            }
        }
    }
}
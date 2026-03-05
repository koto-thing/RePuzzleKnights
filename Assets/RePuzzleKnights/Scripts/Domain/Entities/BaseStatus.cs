using R3;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Domain.Entities
{
    public class BaseStatus
    {
        public int MaxDurability { get; }
        
        public ReadOnlyReactiveProperty<int> CurrentDurability => _currentDurability;
        private readonly ReactiveProperty<int> _currentDurability;

        public Observable<Unit> OnBaseDestroyed => _onBaseDestroyed;
        private readonly Subject<Unit> _onBaseDestroyed = new();

        public BaseStatus(int maxDurability)
        {
            MaxDurability = maxDurability;
            _currentDurability = new ReactiveProperty<int>(maxDurability);
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="damage">受けるダメージ</param>
        public void TakeDamage(int damage)
        {
            if (damage <= 0 || _currentDurability.Value <= 0) 
                return;

            int next = Mathf.Max(0, _currentDurability.Value - damage);
            _currentDurability.Value = next;

            if (_currentDurability.Value <= 0)
            {
                _onBaseDestroyed.OnNext(Unit.Default);
            }
        }
    }
}



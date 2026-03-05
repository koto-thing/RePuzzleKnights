using R3;
using RePuzzleKnights.Scripts.Domain.Entities;

namespace RePuzzleKnights.Scripts.Application.InGame
{
    public class BaseStatusUseCase
    {
        private readonly BaseStatus _baseStatus;

        public ReadOnlyReactiveProperty<int> CurrentDurability => _baseStatus.CurrentDurability;
        public Observable<Unit> OnBaseDestroyed => _baseStatus.OnBaseDestroyed;
        public int MaxDurability => _baseStatus.MaxDurability;

        public BaseStatusUseCase(BaseStatus baseStatus)
        {
            this._baseStatus = baseStatus;
        }

        public void TakeDamage(int damage)
        {
            _baseStatus.TakeDamage(damage);
        }
    }
}



using RePuzzleKnights.Scripts.Application.InGame;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    public class BaseStatusController
    {
        private readonly BaseStatusUseCase _useCase;

        public BaseStatusController(BaseStatusUseCase useCase)
        {
            this._useCase = useCase;
        }
        
        public void TakeDamage(int damage)
        {
            _useCase.TakeDamage(damage);
        }
    }
}



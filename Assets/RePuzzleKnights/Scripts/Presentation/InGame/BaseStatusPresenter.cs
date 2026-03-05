using System;
using R3;
using RePuzzleKnights.Scripts.Application.InGame;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    public class BaseStatusPresenter : IInitializable, IDisposable
    {
        private readonly BaseStatusUseCase _useCase;
        private readonly IBaseStatusView _view;
        private readonly CompositeDisposable _disposables = new();

        public BaseStatusPresenter(BaseStatusUseCase useCase, IBaseStatusView view)
        {
            this._useCase = useCase;
            this._view = view;
        }

        public void Initialize()
        {
            _useCase.CurrentDurability
                .Subscribe(current => _view.UpdateDurability(current, _useCase.MaxDurability))
                .AddTo(_disposables);

            _useCase.OnBaseDestroyed
                .Subscribe(_ => _view.PlayDestroyEffect())
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}



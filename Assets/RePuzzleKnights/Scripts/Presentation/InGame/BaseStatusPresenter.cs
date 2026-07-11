using System;
using R3;
using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Services;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    public class BaseStatusPresenter : IInitializable, IDisposable
    {
        private readonly BaseStatusUseCase _useCase;
        private readonly ILevelStatusProvider _levelStatusProvider;
        private readonly IBaseStatusView _view;
        private readonly CompositeDisposable _disposables = new();

        public BaseStatusPresenter(
            BaseStatusUseCase useCase,
            ILevelStatusProvider levelStatusProvider,
            IBaseStatusView view)
        {
            this._useCase = useCase;
            this._levelStatusProvider = levelStatusProvider;
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

            _levelStatusProvider.DefeatedEnemyCount
                .Subscribe(defeated =>
                {
                    _view.UpdateEnemyCount(defeated, _levelStatusProvider.TotalEnemyCount);
                })
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}



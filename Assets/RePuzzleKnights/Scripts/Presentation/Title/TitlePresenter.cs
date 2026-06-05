using System;
using Cysharp.Threading.Tasks;
using R3;
using RePuzzleKnights.Scripts.Application.Title;
using UnityEditor;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Presentation.Title
{
    public class TitlePresenter : IInitializable, IDisposable
    {
        private readonly TitleUseCase _useCase;
        private readonly TitleSceneTransitionUseCase _sceneTransitionUseCase;
        private readonly TitleView _view;
        
        private readonly CompositeDisposable _disposables = new();

        public TitlePresenter(
            TitleUseCase useCase, 
            TitleSceneTransitionUseCase sceneTransitionUseCase,
            TitleView view
            )
        {
            _useCase = useCase; 
            _sceneTransitionUseCase = sceneTransitionUseCase;
            _view = view;
        }

        public void Initialize()
        {
            _view.OnStartButtonClicked
                .Subscribe(_ => _sceneTransitionUseCase.TransitionToStageSelectSceneAsync().Forget());

            // TODO: 設定画面

            _view.OnQuitButtonClicked
                .Subscribe(_ => _useCase.EndGame());
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}



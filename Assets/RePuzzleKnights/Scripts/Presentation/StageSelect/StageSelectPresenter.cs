using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using RePuzzleKnights.Scripts.Application.StageSelect;
using RePuzzleKnights.Scripts.Domain.Repositories;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Presentation.StageSelect
{
    public class StageSelectPresenter : IInitializable, IDisposable
    {
        private readonly StageSelectUseCase _useCase;
        private readonly StageSelectView _view;
        private readonly IStageRepository _repository;
        private readonly StageSelectSceneTransitionUseCase _sceneTransitionUseCase;
        private readonly CompositeDisposable _disposables = new();

        public StageSelectPresenter(
            StageSelectUseCase useCase,
            StageSelectView view,
            IStageRepository repository,
            StageSelectSceneTransitionUseCase sceneTransitionUseCase)
        {
            _useCase = useCase;
            _view = view;
            _repository = repository; 
            _sceneTransitionUseCase = sceneTransitionUseCase;
        }

        public void Initialize()
        {
            // ステージデータをロード
            _useCase.LoadStagesAsync().Forget();

            // Viewのイベントを購読
            SubscribeViewEvents();
            
            // UseCaseの状態変更をViewに反映
            SubscribeUseCaseEvents();
        }

        private void SubscribeViewEvents()
        {
            _view.OnBackToTitleButtonClicked
                .Subscribe(_ => _sceneTransitionUseCase.TransitionToTitleSceneAsync().Forget())
                .AddTo(_disposables);
            
            // ステージボタンのクリック
            _view.OnStageButtonClicked
                .Subscribe(stageId => _useCase.SelectStage(stageId))
                .AddTo(_disposables);

            // スタートボタンのクリック
            _view.OnStartButtonClicked
                .Subscribe(_ => _useCase.StartSelectedStage())
                .AddTo(_disposables);
        }

        private void SubscribeUseCaseEvents()
        {
            // ステージリストが更新されたらViewに反映
            _useCase.Stages
                .Where(stages => stages != null)
                .Subscribe(stages => 
                {
                    var stageList = stages.ToList();
                    _view.SetStageList(stageList);
                })
                .AddTo(_disposables);

            // 選択されたステージが変更されたらViewに反映
            _useCase.SelectedStage
                .Where(stage => stage != null)
                .Subscribe(async stage =>
                {
                    var sprite = await _repository.GetStageImageAsync(stage.Id);
                    var chara = await _repository.GetCharaImageAsync(stage.Id);
                    _view.SetSelectedStage(stage, sprite, chara);
                })
                .AddTo(_disposables);

            // ステージ開始時の処理
            _useCase.OnStageStarted
                .Subscribe(async stage =>
                {
                    _view.SetInteractable(false);
                    await _sceneTransitionUseCase.TransitionToStage(stage.Id);
                })
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}

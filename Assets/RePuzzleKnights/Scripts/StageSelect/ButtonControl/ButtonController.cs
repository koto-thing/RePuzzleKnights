using System;
using R3;
using RePuzzleKnights.Scripts.StageSelect.StageSelect;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.StageSelect.ButtonControl
{
    public class ButtonController : IInitializable, IDisposable
    {
        private ButtonModel model;
        private ButtonView view;
        private StageSelectModel stageSelectModel;

        private CompositeDisposable disposables = new ();
        
        public ButtonController(ButtonModel model, ButtonView view, StageSelectModel stageSelectModel)
        {
            this.model = model;
            this.view = view;
            this.stageSelectModel = stageSelectModel;
        }

        public void Initialize()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            view.OnBackButtonClicked
                .Subscribe(_ => model.OnBackButtonClicked())
                .AddTo(disposables);
            
            view.OnStage0ButtonClicked
                .Subscribe(_ => { stageSelectModel.LoadStageDataAsync(0); })
                .AddTo(disposables);

            view.OnStage1ButtonClicked
                .Subscribe(_ => { stageSelectModel.LoadStageDataAsync(1); })
                .AddTo(disposables);
            
            view.OnStage2ButtonClicked
                .Subscribe(_ => { stageSelectModel.LoadStageDataAsync(2); })
                .AddTo(disposables);
            
            view.OnStage3ButtonClicked
                .Subscribe(_ => { stageSelectModel.LoadStageDataAsync(3); })
                .AddTo(disposables);
            
            view.OnStage4ButtonClicked
                .Subscribe(_ => { stageSelectModel.LoadStageDataAsync(4); })
                .AddTo(disposables);
            
            view.OnStage5ButtonClicked
                .Subscribe(_ => { stageSelectModel.LoadStageDataAsync(5); })
                .AddTo(disposables);
            
            stageSelectModel.OnLoadStageSelectData
                .Subscribe(_ => view.ShowStageSelectButtons(stageSelectModel.CurrentProgress))
                .AddTo(disposables);
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
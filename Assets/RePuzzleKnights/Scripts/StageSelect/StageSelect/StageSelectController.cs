using System;
using R3;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.StageSelect.StageSelect
{
    public class StageSelectController : IInitializable, IDisposable
    {
        private StageSelectModel model;
        private StageSelectView view;
        
        private CompositeDisposable disposables = new ();

        public StageSelectController(StageSelectModel model, StageSelectView view)
        {
            this.model = model;
            this.view = view;
        }
        
        public void Initialize()
        {
            model.LoadStageSelectDataAsync();
            
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            model.OnChangeStageDescriptionData
                .Skip(1)
                .Subscribe(stageData =>
                {
                    view.SetStageData(stageData);
                    view.ShowStageSelectPanel();
                })
                .AddTo(disposables);

            view.OnStartButtonClicked
                .Subscribe(_ =>
                {
                    model.LoadStageSceneAsync();
                })
                .AddTo(disposables);
            
            view.OnBackButtonClicked
                .Subscribe(_ =>
                {
                    view.HideStageSelectPanel();
                })
                .AddTo(disposables);
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
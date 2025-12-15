using System;
using R3;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Title.ButtonControl
{
    public class ButtonController : IInitializable, IDisposable
    {
        private ButtonModel model;
        private ButtonView view;

        private CompositeDisposable disposables = new ();

        public ButtonController(ButtonModel model, ButtonView view)
        {
            this.model = model;
            this.view = view;
        }
        
        public void Initialize()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            view.OnStartButtonClicked
                .Subscribe(_ => { model.OnStartButtonClicked(); })
                .AddTo(disposables);

            view.OnSettingsButtonClicked
                .Subscribe(_ =>
                {
                    
                })
                .AddTo(disposables);
            
            view.OnQuitButtonClicked
                .Subscribe(_ =>
                {
                    
                })
                .AddTo(disposables);
        }

        public void Dispose()
        {
            
        }
    }
}
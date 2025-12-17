using System;
using R3;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.InGame.BaseSystem
{
    public class BaseStatusController : IInitializable, IDisposable
    {
        private BaseStatusModel model;
        private BaseStatusView view;

        private CompositeDisposable disposables = new ();

        public BaseStatusController(BaseStatusModel model, BaseStatusView view)
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
            model.CurrentDurability
                .Subscribe(durability =>
                {
                    view.UpdateDurabilityDisplay(durability, model.MaxDurability);
                })
                .AddTo(disposables);
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
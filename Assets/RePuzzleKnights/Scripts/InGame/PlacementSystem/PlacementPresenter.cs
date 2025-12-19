using System;
using Cysharp.Threading.Tasks;
using R3;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.InGame.PlacementSystem
{
    public class PlacementPresenter : IInitializable, IDisposable
    {
        private readonly PlacementModel model;
        private readonly PlacementView view;
        
        private CompositeDisposable disposables = new();

        public PlacementPresenter(PlacementModel model, PlacementView view)
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
            model.SelectedAlly
                .Where(data => data != null)
                .Subscribe(data => 
                {
                    view.SpawnPreviewAsync(data, model.PreviewPosition.CurrentValue).Forget();
                })
                .AddTo(disposables);

            model.PreviewPosition
                .Subscribe(pos =>
                {
                    if (view.CurrentPreviewObject != null)
                        view.CurrentPreviewObject.transform.position = pos;
                })
                .AddTo(disposables);

            model.PreviewRotation
                .Subscribe(rot =>
                {
                    if (view.CurrentPreviewObject != null)
                        view.CurrentPreviewObject.transform.rotation = rot;
                })
                .AddTo(disposables);
            
            model.IsValidPosition
                .Subscribe(isValid =>
                {
                    view.UpdatePreviewVisuals(isValid);
                })
                .AddTo(disposables);
            
            model.OnPlacementConfirmed
                .Subscribe(payload =>
                {
                    view.CreateAllyAsync(payload.data, payload.position, payload.rotation, data => model.NotifyAllyDefeated(data)).Forget();
                })
                .AddTo(disposables);

            model.OnCanceled
                .Merge(model.OnPlacementConfirmed.Select(_ => Unit.Default))
                .Subscribe(_ =>
                {
                    view.DestroyPreview();
                })
                .AddTo(disposables);
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}


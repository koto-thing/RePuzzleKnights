using System;
using R3;
using RePuzzleKnights.Scripts.InGame.BaseSystem;
using UnityEngine;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.InGame.GameFlowSystem
{
    public class GameFlowController : IInitializable, IDisposable
    {
        private readonly BaseStatusModel baseStatusModel;

        private CompositeDisposable disposables = new ();

        public GameFlowController(BaseStatusModel baseStatusModel)
        {
            this.baseStatusModel = baseStatusModel;
        }

        public void Initialize()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            baseStatusModel.OnBaseDestroyed
                .Subscribe(_ => Debug.Log("GameOver"))
                .AddTo(disposables);
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
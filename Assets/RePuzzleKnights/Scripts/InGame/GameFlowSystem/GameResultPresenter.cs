using System;
using R3;
using RePuzzleKnights.Scripts.InGame.GameFlowSystem.Enum;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.InGame.GameFlowSystem
{
    /// <summary>
    /// ゲーム結果表示のPresenterクラス
    /// ModelとViewの連携を管理
    /// </summary>
    public class GameResultPresenter : IInitializable, IDisposable
    {
        private readonly GameFlowModel model;
        private readonly GameResultView view;
        
        private readonly CompositeDisposable disposables = new();
        
        public GameResultPresenter(GameFlowModel model, GameResultView view)
        {
            this.model = model;
            this.view = view;
        }

        public void Initialize()
        {
            SubscribeEvents();
        }

        /// <summary>
        /// イベント購読の設定
        /// </summary>
        private void SubscribeEvents()
        {
            model.CurrentState
                .Where(state => state != GameResultState.PLAYING)
                .Subscribe(state =>
                {
                    view.ShowResult(state);
                })
                .AddTo(disposables);
        }

        public void Dispose()
        {
            disposables?.Dispose();
        }
    }
}


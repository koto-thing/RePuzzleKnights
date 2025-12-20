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
        private readonly GameFlowSoundEmitter soundEmitter;
        
        private readonly CompositeDisposable disposables = new();
        
        public GameResultPresenter(GameFlowModel model, GameResultView view, GameFlowSoundEmitter soundEmitter)
        {
            this.model = model;
            this.view = view;
            this.soundEmitter = soundEmitter;
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
                    // BGMを停止
                    soundEmitter?.StopBgm();
                    
                    // 効果音を再生
                    if (state == GameResultState.GAME_CLEAR)
                    {
                        soundEmitter?.PlayStageClearSe();
                    }
                    else if (state == GameResultState.GAME_OVER)
                    {
                        soundEmitter?.PlayGameOverSe();
                    }
                    
                    // 結果画面を表示
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


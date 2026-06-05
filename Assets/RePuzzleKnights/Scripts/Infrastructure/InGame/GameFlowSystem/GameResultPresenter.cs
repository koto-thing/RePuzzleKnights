using System;
using R3;
using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Enums;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.GameFlowSystem
{
    /// <summary>
    /// ゲーム結果表示のPresenterクラス
    /// ModelとViewの連携を管理
    /// </summary>
    public class GameResultPresenter : IInitializable, IDisposable
    {
        private readonly GameFlowUseCase _useCase;
        private readonly GameResultView _view;
        private readonly GameFlowSoundEmitter _soundEmitter;
        
        private readonly CompositeDisposable _disposables = new();
        
        public GameResultPresenter(GameFlowUseCase useCase, GameResultView view, GameFlowSoundEmitter soundEmitter)
        {
            this._useCase = useCase;
            this._view = view;
            this._soundEmitter = soundEmitter;
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
            _useCase.CurrentState
                .Where(state => state != GameResultState.PLAYING)
                .Subscribe(state =>
                {
                    // BGMを停止
                    _soundEmitter?.StopBgm();
                    
                    // 効果音を再生
                    if (state == GameResultState.GAME_CLEAR)
                    {
                        _soundEmitter?.PlayStageClearSe();
                    }
                    else if (state == GameResultState.GAME_OVER)
                    {
                        _soundEmitter?.PlayGameOverSe();
                    }
                    
                    // 結果画面を表示
                    _view.ShowResult(state);
                })
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}





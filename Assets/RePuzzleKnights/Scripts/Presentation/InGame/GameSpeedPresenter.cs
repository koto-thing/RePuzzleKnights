using System;
using R3;
using RePuzzleKnights.Scripts.Application.InGame;
using UnityEngine;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    /// <summary>
    /// ゲームの進行速度制御を仲介するPresenter。
    /// ユースケースの状態を監視し、Time.timeScaleを変更した上でViewを更新する。
    /// </summary>
    public class GameSpeedPresenter : IStartable, IDisposable
    {
        private readonly GameSpeedUseCase _useCase;
        private readonly ISpeedButtonView _view;
        private readonly CompositeDisposable _disposables = new();

        public GameSpeedPresenter(GameSpeedUseCase useCase, ISpeedButtonView view)
        {
            _useCase = useCase;
            _view = view;
        }

        public void Start()
        {
            _view.OnClick
                .Subscribe(_ =>
                {
                    _useCase.ToggleSpeed();
                })
                .AddTo(_disposables);

            _useCase.CurrentSpeed
                .Subscribe(speed =>
                {
                    // UnityのTime.timeScaleを更新
                    Time.timeScale = speed;
                    
                    // ビューの表示を更新
                    _view.UpdateSpeedVisual(speed);
                    
                    Debug.Log($"[GameSpeedPresenter] Time.timeScale updated: {speed}x");
                })
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            // シーン終了・遷移時は安全のために速度を等速(1倍)に戻す
            Time.timeScale = 1.0f;
            _disposables.Dispose();
        }
    }
}

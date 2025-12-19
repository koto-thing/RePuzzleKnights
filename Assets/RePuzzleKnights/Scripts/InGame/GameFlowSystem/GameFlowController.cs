using System;
using R3;
using RePuzzleKnights.Scripts.InGame.BaseSystem;
using UnityEngine;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.InGame.GameFlowSystem
{
    /// <summary>
    /// ゲーム全体の流れを管理するコントローラークラス
    /// 勝敗判定、ゲームオーバー処理などを担当
    /// </summary>
    public class GameFlowController : IInitializable, IDisposable
    {
        private readonly BaseStatusModel baseStatusModel;

        private CompositeDisposable disposables = new ();

        public GameFlowController(BaseStatusModel baseStatusModel)
        {
            this.baseStatusModel = baseStatusModel;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize()
        {
            SubscribeEvents();
        }

        /// <summary>
        /// イベント購読の設定
        /// </summary>
        private void SubscribeEvents()
        {
            baseStatusModel.OnBaseDestroyed
                .Subscribe(_ => OnGameOver())
                .AddTo(disposables);
        }

        /// <summary>
        /// ゲームオーバー時の処理
        /// </summary>
        private void OnGameOver()
        {
            Debug.Log("GameOver");
            // TODO: ゲームオーバー画面への遷移など
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
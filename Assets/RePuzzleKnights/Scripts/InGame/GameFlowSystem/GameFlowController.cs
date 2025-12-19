using System;
using R3;
using RePuzzleKnights.Scripts.InGame.BaseSystem;
using RePuzzleKnights.Scripts.InGame.GameFlowSystem.Enum;
using RePuzzleKnights.Scripts.InGame.GameFlowSystem.Interface;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.InGame.GameFlowSystem
{
    /// <summary>
    /// ゲームフローのコントローラークラス
    /// ゲームクリア・ゲームオーバー判定を管理
    /// </summary>
    public class GameFlowController : IInitializable, IDisposable
    {
        private readonly GameFlowModel model;
        private readonly BaseStatusModel baseStatusModel;
        private readonly IWaveInfoProvider waveInfoProvider;
        private readonly IEnemyInfoProvider enemyInfoProvider;
        
        private readonly CompositeDisposable disposables = new();
        
        public GameFlowController(
            GameFlowModel model,
            BaseStatusModel baseStatusModel,
            IWaveInfoProvider waveInfoProvider,
            IEnemyInfoProvider enemyInfoProvider)
        {
            this.model = model;
            this.baseStatusModel = baseStatusModel;
            this.waveInfoProvider = waveInfoProvider;
            this.enemyInfoProvider = enemyInfoProvider;
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
            // ゲームオーバー判定: 本拠地が破壊された
            baseStatusModel.OnBaseDestroyed
                .Subscribe(_ =>
                {
                    UnityEngine.Debug.Log("[GameFlowController] Base destroyed - Game Over");
                    model.TransitionState(GameResultState.GAME_OVER);
                })
                .AddTo(disposables);
            
            // ゲームクリア判定: すべてのウェーブが終了し、敵が全滅した
            Observable.CombineLatest(
                    waveInfoProvider.IsAllWavesFinished,
                    enemyInfoProvider.ActiveEnemyCount,
                    (isWaveFinished, enemyCount) => new { isWaveFinished, enemyCount }
                )
                .Where(x => x.isWaveFinished && x.enemyCount == 0)
                .Take(1) // 一度だけ発火
                .Subscribe(_ =>
                {
                    UnityEngine.Debug.Log("[GameFlowController] All waves cleared and no enemies - Game Clear");
                    model.TransitionState(GameResultState.GAME_CLEAR);
                })
                .AddTo(disposables);
        }

        public void Dispose()
        {
            disposables?.Dispose();
        }
    }
}
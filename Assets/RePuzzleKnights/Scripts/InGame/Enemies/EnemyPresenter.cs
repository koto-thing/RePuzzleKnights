using System;
using R3;
using RePuzzleKnights.Scripts.InGame.BaseSystem;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    /// <summary>
    /// 敵キャラクターのPresenterクラス
    /// Model、Controller、Viewの連携を管理し、イベントのバインディングを行う
    /// </summary>
    public class EnemyPresenter : IDisposable
    {
        private readonly EnemyModel model;
        private readonly EnemyController controller;
        private readonly EnemyView view;
        private readonly BaseStatusModel baseStatusModel;
        
        private readonly CompositeDisposable disposables = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EnemyPresenter(
            EnemyModel model,
            EnemyController controller,
            EnemyView view,
            BaseStatusModel baseStatusModel)
        {
            this.model = model;
            this.controller = controller;
            this.view = view;
            this.baseStatusModel = baseStatusModel;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize()
        {
            // Viewにコントローラーを設定
            view.SetController(controller);
            
            // ステータス表示の初期化
            view.InitializeStatusDisplay(model.Status, model.Data.MaxHp);
            
            // イベント購読
            SubscribeEvents();
        }

        /// <summary>
        /// イベント購読の設定
        /// </summary>
        private void SubscribeEvents()
        {
            // 移動ターゲットが変わったらViewに移動指示
            model.Mover.CurrentTarget
                .Subscribe(target =>
                {
                    view.MoveTo(target, model.Data.MoveSpeed, () => model.Mover.OnArrivedAtTarget());
                })
                .AddTo(disposables);

            // ブロック状態の変化に応じてViewを制御
            model.CurrentBlocker
                .Subscribe(blocker =>
                {
                    if (blocker != null)
                    {
                        // ブロックされた時: 移動を一時停止
                        view.PauseMove();
                    }
                    else
                    {
                        // ブロック解除時: 現在位置から次の目標地点へ新しく移動開始
                        view.RestartMove(model.Mover.CurrentTarget.CurrentValue, model.Data.MoveSpeed, 
                            () => model.Mover.OnArrivedAtTarget());
                    }
                })
                .AddTo(disposables);

            // HPが変化したらダメージエフェクト再生（減少時のみ）
            model.Status.CurrentHp
                .Pairwise()
                .Where(pair => pair.Previous > pair.Current)
                .Subscribe(_ => view.PlayDamageEffect())
                .AddTo(disposables);

            // 死亡時の処理
            model.Status.IsDead
                .Where(dead => dead)
                .Subscribe(_ =>
                {
                    view.DestroyActor();
                    Dispose();
                })
                .AddTo(disposables);

            // ゴール到達時の処理
            model.Mover.OnGoalReached
                .Subscribe(_ =>
                {
                    // 本拠地にダメージ
                    baseStatusModel.TakeDamage(1);
                    
                    view.DestroyActor();
                    Dispose();
                })
                .AddTo(disposables);

            // 位置調整イベント（ブロック時のずらし処理）
            // 複数の敵がブロックされた際に重ならないように配置
            model.OnTargetPositionChanged
                .Subscribe(data =>
                {
                    view.MoveToPosition(data.position, data.duration);
                })
                .AddTo(disposables);
        }

        public void Dispose()
        {
            disposables.Dispose();
            controller.Dispose();
        }
    }
}


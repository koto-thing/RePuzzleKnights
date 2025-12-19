using System;
using R3;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.InGame.BaseSystem
{
    /// <summary>
    /// 本拠地ステータスのコントローラークラス
    /// ModelとViewの連携を管理
    /// </summary>
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
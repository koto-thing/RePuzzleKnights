using RePuzzleKnights.Scripts.Common;
using RePuzzleKnights.Scripts.StageSelect.ButtonControl;
using RePuzzleKnights.Scripts.StageSelect.StageSelect;
using VContainer;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.StageSelect
{
    /// <summary>
    /// ステージ選択シーンのDIコンテナ設定
    /// </summary>
    public class StageSelectLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // 共通サービス
            builder.Register<StageProgressService>(Lifetime.Singleton);
            builder.Register<CurrentStageService>(Lifetime.Singleton);
            
            // ボタン機能
            builder.Register<ButtonModel>(Lifetime.Singleton);
            builder.RegisterEntryPoint<ButtonController>();
            builder.RegisterComponentInHierarchy<ButtonView>();
            
            // ステージセレクト機能
            builder.Register<StageSelectModel>(Lifetime.Singleton);
            builder.RegisterEntryPoint<StageSelectController>();
            builder.RegisterComponentInHierarchy<StageSelectView>();
        }
    }
}
using RePuzzleKnights.Scripts.StageSelect.ButtonControl;
using RePuzzleKnights.Scripts.StageSelect.StageSelect;
using VContainer;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.StageSelect
{
    public class StageSelectLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
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
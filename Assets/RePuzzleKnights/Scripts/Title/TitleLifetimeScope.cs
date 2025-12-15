using RePuzzleKnights.Scripts.Title.ButtonControl;
using VContainer;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Title
{
    public class TitleLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // ボタン機能
            builder.Register<ButtonModel>(Lifetime.Singleton);
            builder.RegisterEntryPoint<ButtonController>();
            builder.RegisterComponentInHierarchy<ButtonView>();
        }
    }
}
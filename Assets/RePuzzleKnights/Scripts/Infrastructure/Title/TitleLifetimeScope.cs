using RePuzzleKnights.Scripts.Application.Title;
using RePuzzleKnights.Scripts.Infrastructure.Common;
using RePuzzleKnights.Scripts.Presentation.Title;
using VContainer;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Infrastructure.Title
{
    public class TitleLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<TitleUseCase>(Lifetime.Scoped);
            builder.Register<TitleSceneTransitionUseCase>(Lifetime.Scoped);
            
            builder.RegisterEntryPoint<TitlePresenter>();

            builder.RegisterComponentInHierarchy<TitleView>();
        }
    }
}


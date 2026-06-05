using RePuzzleKnights.Scripts.Application.Common;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Infrastructure.Common
{
    public class RootLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<AddressablesSceneLoader>(Lifetime.Singleton)
                .As<ISceneLoader>();
        }
    }
}
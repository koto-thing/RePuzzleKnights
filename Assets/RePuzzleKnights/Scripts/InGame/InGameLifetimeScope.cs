using RePuzzleKnights.Scripts.InGame.PathFinder;
using VContainer;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.InGame
{
    public class InGameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // 経路探索システム
            builder.Register<AStarPathFinder>(Lifetime.Singleton);
            builder.Register<PathFinderModel>(Lifetime.Singleton);
            builder.Register<Graph>(Lifetime.Singleton);
            builder.RegisterEntryPoint<PathFinderController>();
            builder.RegisterComponentInHierarchy<PathFinderView>();
            builder.RegisterComponentInHierarchy<GraphCreator>();
        }
    }
}
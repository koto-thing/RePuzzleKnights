using RePuzzleKnights.Scripts.Common;
using RePuzzleKnights.Scripts.InGame.Enemies;
using RePuzzleKnights.Scripts.InGame.BaseSystem;
using RePuzzleKnights.Scripts.InGame.GameFlowSystem;
using RePuzzleKnights.Scripts.InGame.PathFinder;
using RePuzzleKnights.Scripts.InGame.PlacementSystem;
using VContainer;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.InGame
{
    /// <summary>
    /// InGameシーンのDIコンテナ設定
    /// VContainerを使用してシステム間の依存関係を管理
    /// </summary>
    public class InGameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // 共通サービス
            builder.Register<StageProgressService>(Lifetime.Singleton);
            builder.Register<CurrentStageService>(Lifetime.Singleton);
            
            // 経路探索システム
            builder.Register<AStarPathFinder>(Lifetime.Singleton);
            builder.Register<PathFinderModel>(Lifetime.Singleton);
            builder.Register<Graph>(Lifetime.Singleton);
            builder.RegisterEntryPoint<PathFinderController>();
            builder.RegisterComponentInHierarchy<PathFinderView>();
            builder.RegisterComponentInHierarchy<GraphCreator>();
            
            // 配置システム
            builder.Register<PlacementInputService>(Lifetime.Singleton);
            builder.Register<PlacementModel>(Lifetime.Singleton);
            builder.Register<PlacementController>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.RegisterEntryPoint<PlacementPresenter>();
            builder.RegisterComponentInHierarchy<PlacementView>();
            
            // 敵システム
            builder.Register<EnemyFactory>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<EnemySpawner>().AsImplementedInterfaces();
            
            // 耐久値管理システム
            builder.Register<BaseStatusModel>(Lifetime.Singleton);
            builder.RegisterEntryPoint<BaseStatusController>();
            builder.RegisterComponentInHierarchy<BaseStatusView>();
            
            // ゲームフロー管理システム
            builder.Register<GameFlowModel>(Lifetime.Singleton);
            builder.RegisterEntryPoint<GameFlowController>();
            builder.RegisterEntryPoint<GameResultPresenter>();
            builder.RegisterComponentInHierarchy<GameResultView>();
        }
    }
}

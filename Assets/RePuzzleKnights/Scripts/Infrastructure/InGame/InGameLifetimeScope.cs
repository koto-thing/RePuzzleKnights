using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Domain.Repositories;
using RePuzzleKnights.Scripts.Domain.Services;
using RePuzzleKnights.Scripts.Infrastructure.Common;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies;
using RePuzzleKnights.Scripts.Infrastructure.InGame.GameFlowSystem;
using RePuzzleKnights.Scripts.Infrastructure.InGame.PathFinder;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Placement;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Soul;
using RePuzzleKnights.Scripts.Infrastructure.InGame.UI;
using RePuzzleKnights.Scripts.Presentation.InGame;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame
{
    /// <summary>
    /// InGameシーンのDIコンテナ設定
    /// VContainerを使用してシステム間の依存関係を管理
    /// </summary>
    public class InGameLifetimeScope : LifetimeScope
    {
        [SerializeField] private System.Collections.Generic.List<RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO.AllyDataSO> allyDatas;

        protected override void Configure(IContainerBuilder builder)
        {
            // 共通サービス
            builder.Register<StageProgressService>(Lifetime.Singleton);
            builder.Register<CurrentStageService>(Lifetime.Singleton);
            
            // Soulシステム
            builder.Register<SoulWallet>(Lifetime.Singleton)
                .WithParameter("initialSoul", 10)
                .WithParameter("maxSoul", 999);
            builder.Register<SoulUseCase>(Lifetime.Singleton);
            builder.RegisterEntryPoint<SoulPresenter>();
            builder.RegisterComponentInHierarchy<SoulCostView>().As<ISoulCostView>();
            builder.RegisterComponentInHierarchy<SoulSpawner>();
            
            // 倍速システム
            builder.Register<GameSpeedUseCase>(Lifetime.Singleton);
            builder.RegisterEntryPoint<GameSpeedPresenter>();
            builder.RegisterComponentInHierarchy<SpeedButtonView>().As<ISpeedButtonView>();
            
            // Clean Architecture Registrations
            builder.Register<StageDataRepository>(Lifetime.Singleton).As<IStageRepository>();
            builder.Register<GameFlowUseCase>(Lifetime.Singleton);
            
            builder.Register<AddressableAllyDataRepository>(Lifetime.Singleton)
                .AsSelf()
                .As<IAllyDataRepository>()
                .WithParameter(allyDatas);

            builder.Register<FusionUseCase>(Lifetime.Singleton);
            builder.Register<AllyFactory>(Lifetime.Singleton);

            // キャラクター詳細パネル
            // シーンごとに手動で配置する必要がないよう、詳細パネルは実行時に生成する。
            var detailPanelObject = new GameObject("AllyDetailPanelView");
            detailPanelObject.transform.SetParent(transform, false);
            var detailPanelView = detailPanelObject.AddComponent<AllyDetailPanelView>();
            builder.RegisterComponent(detailPanelView).As<IAllyDetailPanelView>();
            builder.Register<AllyDetailPresenter>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            // 経路探索システム
            builder.Register<NativePathFinder>(Lifetime.Singleton).As<IPathFinderService>();
            builder.Register<FindPathUseCase>(Lifetime.Singleton);
            builder.Register<Graph>(Lifetime.Singleton);
            
            builder.RegisterEntryPoint<PathFinderController>();
            
            builder.RegisterComponentInHierarchy<PathFinderView>();
            builder.RegisterComponentInHierarchy<GraphCreator>();
            
            // 配置システム
            builder.Register<PlacementUseCase>(Lifetime.Singleton);
            builder.Register<UnityPlacementValidator>(Lifetime.Singleton).As<IPlacementValidator>();
            builder.Register<PlacementInputService>(Lifetime.Singleton);
            
            builder.Register<PlacementController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<PlacementPresenter>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            
            builder.RegisterComponentInHierarchy<PlacementView>().As<IPlacementView>();
            builder.RegisterComponentInHierarchy<PlacementConnector>();
            
            // 敵システム
            builder.Register<EnemyFactory>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<EnemySpawner>()
                .As<ILevelStatusProvider>();
            
            builder.Register<BaseStatus>(Lifetime.Singleton).WithParameter(10);
            builder.Register<BaseStatusUseCase>(Lifetime.Singleton);
            builder.Register<BaseStatusController>(Lifetime.Singleton);
            builder.RegisterEntryPoint<BaseStatusPresenter>();
            
            builder.RegisterComponentInHierarchy<BaseStatusView>().As<IBaseStatusView>();
            
            builder.RegisterEntryPoint<GameFlowController>();
            
            builder.RegisterEntryPoint<GameResultPresenter>();
            builder.RegisterComponentInHierarchy<GameResultView>();
            builder.RegisterComponentInHierarchy<GameFlowSoundEmitter>();
        }
    }
}



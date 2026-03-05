using RePuzzleKnights.Scripts.Application.Common;
using RePuzzleKnights.Scripts.Application.StageSelect;
using RePuzzleKnights.Scripts.Domain.Repositories;
using RePuzzleKnights.Scripts.Infrastructure.Common;
using RePuzzleKnights.Scripts.Infrastructure.StageSelect.Definitions;
using RePuzzleKnights.Scripts.Presentation.StageSelect;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Infrastructure.StageSelect
{
    public class StageSelectLifetimeScope : LifetimeScope
    {
        [SerializeField] private StageSelectDataSO stageSelectData;
        
        protected override void Configure(IContainerBuilder builder)
        { 
            // Common Services
            builder.Register<StageProgressService>(Lifetime.Scoped);
            
            // Config
            builder.RegisterInstance(stageSelectData);
            
            // Repository
            builder.Register<StageRepository>(Lifetime.Scoped).As<IStageRepository>();
            
            // UseCase
            builder.Register<StageSelectUseCase>(Lifetime.Scoped);
            builder.Register<StageSelectSceneTransitionUseCase>(Lifetime.Scoped);
            
            builder.RegisterComponentInHierarchy<StageSelectView>();
            builder.RegisterEntryPoint<StageSelectPresenter>();
        }
    }
}

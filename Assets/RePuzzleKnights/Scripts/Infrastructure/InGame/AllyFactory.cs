using Cysharp.Threading.Tasks;
using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Domain.Enums;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO;
using UnityEngine;
using UnityEngine.AddressableAssets;
// Alias types to distinguish between Infrastructure and Domain enums
using DomainAttackPriority = RePuzzleKnights.Scripts.Domain.Entities.AttackPriority;
using DomainAttackRangeType = RePuzzleKnights.Scripts.Domain.Entities.AttackRangeType;
using DomainAllyType = RePuzzleKnights.Scripts.Domain.Entities.AllyType;
using InfraAllyType = RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.Enum.AllyType;
using InfraAttackRangeType = RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO.AttackRangeType;
using InfraAttackPriority = RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO.AttackPriority;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame
{
    public class AllyFactory
    {
        private readonly PlacementUseCase _placementUseCase;

        public AllyFactory(PlacementUseCase placementUseCase)
        {
             this._placementUseCase = placementUseCase;
        }

        public async UniTask<Ally> CreateAllyAsync(AllyDataSO data, Vector3 position, Quaternion rotation)
        {
            var handle = Addressables.InstantiateAsync(data.PrefabRef);
            var obj = await handle.ToUniTask();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            
            var battlePresenter = obj.GetComponent<AllyBattlePresenter>();
            if (battlePresenter != null)
            {
                battlePresenter.Initialize(allyData => {
                    _placementUseCase.NotifyAllyDefeated(allyData.AllyName);
                });
            }
            else
            {
                Debug.LogWarning("AllyBattlePresenter not found on prefab. Battle logic will not work.");
            }
            
            var view = obj.GetComponent<AllyView>();
            if (view != null)
            {
                view.SetInitialDirection(rotation);
            }
            
            var stats = MapToStats(data);
            var ally = new Ally(System.Guid.NewGuid().ToString(), stats);

            return ally;
        }

        public AllyStats CreateStats(AllyDataSO data)
        {
            return MapToStats(data);
        }

        private AllyStats MapToStats(AllyDataSO data)
        {
            var rType = MapRangeType(data.RangeType);
            var priority = MapPriority(data.Priority);
            var placementType = MapPlacementType(data.AllyType);
            
            var allyType = DomainAllyType.MELEE; 
            
            return new AllyStats(
                data.AllyName,
                placementType,
                allyType,
                data.MaxHp,
                data.AttackPower,
                data.AttackRange,
                data.AttackInterval,
                data.BlockCount,
                data.SearchRadius,
                rType,
                priority,
                data.SplashRadius,
                data.CanAttackFlying
            );
        }
        
        private PlacementType MapPlacementType(InfraAllyType type)
        {
            switch(type)
            {
                case InfraAllyType.Ground: return PlacementType.Ground;
                case InfraAllyType.HighGround: return PlacementType.HighGround;
                default: return PlacementType.Ground;
            }
        }

        private DomainAttackRangeType MapRangeType(InfraAttackRangeType type)
        {
            switch(type)
            {
                case InfraAttackRangeType.SINGLE_TARGET: return DomainAttackRangeType.SINGLE_TARGET;
                case InfraAttackRangeType.SPLASH_AROUND_TARGET: return DomainAttackRangeType.SPLASH_AROUND_TARGET;
                case InfraAttackRangeType.FULL_RANGE_AREA: return DomainAttackRangeType.FULL_RANGE_AREA;
                default: return DomainAttackRangeType.SINGLE_TARGET;
            }
        }

        private DomainAttackPriority MapPriority(InfraAttackPriority priority)
        {
             switch(priority)
            {
                case InfraAttackPriority.CLOSEST: return DomainAttackPriority.CLOSEST;
                case InfraAttackPriority.FLYING_PRIORITIZED: return DomainAttackPriority.FLYING_PRIORITIZED;
                case InfraAttackPriority.BLOCK_ONLY: return DomainAttackPriority.BLOCK_ONLY;
                default: return DomainAttackPriority.CLOSEST;
            }
        }
    }
}

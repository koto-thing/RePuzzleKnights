using Cysharp.Threading.Tasks;
using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Domain.Enums;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
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
        private readonly FusionUseCase _fusionUseCase;
        private readonly AddressableAllyDataRepository _allyDataRepository;
        private readonly IObjectResolver _container;

        public AllyFactory(
            PlacementUseCase placementUseCase,
            FusionUseCase fusionUseCase,
            AddressableAllyDataRepository allyDataRepository,
            IObjectResolver container)
        {
             this._placementUseCase = placementUseCase;
             this._fusionUseCase = fusionUseCase;
             this._allyDataRepository = allyDataRepository;
             this._container = container;
        }

        public async UniTask<Ally> CreateAllyAsync(AllyDataSO data, Vector3 position, Quaternion rotation)
        {
            if (data.PrefabRef == null || !data.PrefabRef.RuntimeKeyIsValid())
            {
                Debug.LogError($"AllyFactory: Invalid PrefabRef for {data.AllyName}. Please ensure the prefab is assigned in the AllyDataSO or SoulDataSO asset.");
                return null;
            }

            var handle = Addressables.InstantiateAsync(data.PrefabRef);
            var obj = await handle.ToUniTask();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            
            // レイヤーをAllyに設定（融合判定に必須）
            int allyLayer = LayerMask.NameToLayer("Ally");
            if (allyLayer != -1)
            {
                SetLayerRecursively(obj, allyLayer);
            }
            
            // 1. まずエンティティとリファレンスを作成（最優先）
            var stats = MapToStats(data);
            var ally = new Ally(System.Guid.NewGuid().ToString(), stats);

            var reference = obj.GetComponent<AllyReference>();
            if (reference == null) reference = obj.AddComponent<AllyReference>();
            reference.Initialize(ally);

            // 2. その後にプレゼンターを初期化（リファレンスの中身が入っている状態で）
            var battlePresenter = obj.GetComponent<AllyBattlePresenter>();
            if (battlePresenter != null)
            {
                // VContainerの依存注入
                _container.Inject(battlePresenter);

                battlePresenter.Initialize(data, allyData => {
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

            return ally;
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
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
            
            var rangeGrids = new System.Collections.Generic.List<GridCoordinate>();
            if (data.AttackRangeGrids != null)
            {
                foreach (var grid in data.AttackRangeGrids)
                {
                    rangeGrids.Add(new GridCoordinate(grid.x, grid.y));
                }
            }

            StatusEffectType customEffectType = StatusEffectType.None;
            float customEffectDuration = 0f;
            float customEffectValue = 0f;
            float customEffectProbability = 0f;
            float selfRegenPercent = 0f;
            float reflectDamagePercent = 0f;
            float dodgeChance = 0f;
            int maxTargets = 1;
            bool isSplash = false;
            int blockCount = data.BlockCount;

            switch (data.Element)
            {
                case ElementType.Fire:
                    customEffectType = StatusEffectType.Burn;
                    customEffectDuration = 3f;
                    customEffectValue = 10f; // 毎秒10ダメ
                    customEffectProbability = 1f; // 確定
                    break;
                case ElementType.Water:
                    customEffectType = StatusEffectType.Slow;
                    customEffectDuration = 2f;
                    customEffectValue = 0.3f; // 30%減速
                    customEffectProbability = 1f; // 確定
                    break;
                case ElementType.Grass:
                    blockCount = 1; // Lv1盾はブロック1
                    selfRegenPercent = 0.01f; // 1%リジェネ
                    reflectDamagePercent = 0.1f; // 10%反射
                    break;
                case ElementType.Light:
                    customEffectType = StatusEffectType.Stun;
                    customEffectDuration = 0.5f;
                    customEffectProbability = 0.1f; // 10%スタン
                    maxTargets = 1;
                    break;
                case ElementType.Dark:
                    dodgeChance = 0.2f; // 20%物理回避
                    customEffectType = StatusEffectType.DefDebuff;
                    customEffectDuration = 3f;
                    customEffectValue = 10f; // 防御力10低下
                    customEffectProbability = 0.5f; // 50%付与
                    blockCount = 1; // アサシンは常にブロック1
                    break;
            }
            
            return new AllyStats(
                data.AllyName,
                placementType,
                allyType,
                data.Element,
                data.MaxHp,
                data.AttackPower,
                data.AttackRange,
                data.AttackInterval,
                blockCount,
                data.SearchRadius,
                rType,
                priority,
                data.SplashRadius,
                data.CanAttackFlying,
                rangeGrids,
                customEffectType,
                customEffectDuration,
                customEffectValue,
                customEffectProbability,
                selfRegenPercent,
                reflectDamagePercent,
                dodgeChance,
                maxTargets,
                isSplash
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

using RePuzzleKnights.Scripts.Domain.Enums;

namespace RePuzzleKnights.Scripts.Domain.Entities
{
    public enum AttackRangeType { SINGLE_TARGET, SPLASH_AROUND_TARGET, FULL_RANGE_AREA }
    public enum AttackPriority { CLOSEST, FLYING_PRIORITIZED, BLOCK_ONLY }
    public enum AllyType { MELEE, RANGED, SUPPORT }

    public class AllyStats
    {
        public string Name { get; }
        public PlacementType PlacementType { get; }
        public AllyType Type { get; }
        public ElementType Element { get; }
        public float MaxHp { get; }
        public float AttackPower { get; }
        public float AttackRange { get; }
        public float AttackInterval { get; }
        public int BlockCount { get; }
        public float SearchRadius { get; }
        public AttackRangeType RangeType { get; }
        public AttackPriority Priority { get; }
        public float SplashRadius { get; }
        public bool CanAttackFlying { get; }
        public System.Collections.Generic.List<GridCoordinate> AttackRangeGrids { get; }

        // 独自アビリティ関連パラメータ
        public StatusEffectType CustomEffectType { get; }
        public float CustomEffectDuration { get; }
        public float CustomEffectValue { get; }
        public float CustomEffectProbability { get; }
        public float SelfRegenPercent { get; }
        public float ReflectDamagePercent { get; }
        public float DodgeChance { get; }
        public int MaxTargets { get; }
        public bool IsSplash { get; }

        public AllyStats(
            string name, PlacementType placementType, AllyType type, ElementType element,
            float maxHp, float attackPower,
            float attackRange, float attackInterval, int blockCount,
            float searchRadius, AttackRangeType rangeType, AttackPriority priority,
            float splashRadius, bool canAttackFlying,
            System.Collections.Generic.List<GridCoordinate> attackRangeGrids = null,
            StatusEffectType customEffectType = StatusEffectType.None,
            float customEffectDuration = 0f,
            float customEffectValue = 0f,
            float customEffectProbability = 0f,
            float selfRegenPercent = 0f,
            float reflectDamagePercent = 0f,
            float dodgeChance = 0f,
            int maxTargets = 1,
            bool isSplash = false)
        {
            Name = name;
            PlacementType = placementType;
            Type = type;
            Element = element;
            MaxHp = maxHp;
            AttackPower = attackPower;
            AttackRange = attackRange;
            AttackInterval = attackInterval;
            BlockCount = blockCount;
            SearchRadius = searchRadius;
            RangeType = rangeType;
            Priority = priority;
            SplashRadius = splashRadius;
            CanAttackFlying = canAttackFlying;
            AttackRangeGrids = attackRangeGrids ?? new System.Collections.Generic.List<GridCoordinate>();
            
            CustomEffectType = customEffectType;
            CustomEffectDuration = customEffectDuration;
            CustomEffectValue = customEffectValue;
            CustomEffectProbability = customEffectProbability;
            SelfRegenPercent = selfRegenPercent;
            ReflectDamagePercent = reflectDamagePercent;
            DodgeChance = dodgeChance;
            MaxTargets = maxTargets;
            IsSplash = isSplash;
        }
    }
}



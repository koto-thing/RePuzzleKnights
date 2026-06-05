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

        public AllyStats(
            string name, PlacementType placementType, AllyType type, ElementType element,
            float maxHp, float attackPower,
            float attackRange, float attackInterval, int blockCount,
            float searchRadius, AttackRangeType rangeType, AttackPriority priority,
            float splashRadius, bool canAttackFlying)
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
        }
    }
}



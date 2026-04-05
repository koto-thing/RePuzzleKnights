using RePuzzleKnights.Scripts.Domain.Enums;

namespace RePuzzleKnights.Scripts.Domain.Entities
{
    public class EnemyStats
    {
        public string Name { get; }
        public ElementType Element { get; }
        public float MaxHp { get; }
        public float MoveSpeed { get; }
        public float AttackPower { get; }
        public float AttackInterval { get; }
        public EnemyRank Rank { get; }
        public int AttackRange { get; }
        public MovementType MoveType { get; }
        public bool IgnoreTerrain { get; }

        public EnemyStats(
            string name,
            ElementType element,
            float maxHp,
            float moveSpeed,
            float attackPower,
            float attackInterval,
            EnemyRank rank,
            int attackRange,
            MovementType moveType,
            bool ignoreTerrain)
        {
            Name = name;
            Element = element;
            MaxHp = maxHp;
            MoveSpeed = moveSpeed;
            AttackPower = attackPower;
            AttackInterval = attackInterval;
            Rank = rank;
            AttackRange = attackRange;
            MoveType = moveType;
            IgnoreTerrain = ignoreTerrain;
        }
    }
}




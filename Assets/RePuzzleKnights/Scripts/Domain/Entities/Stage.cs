namespace RePuzzleKnights.Scripts.Domain.Entities
{
    public class Stage
    {
        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        public bool IsUnlocked { get; }
        public bool IsCleared { get; }
        
        public Stage(int id, string name, string description, bool isUnlocked, bool isCleared)
        {
            Id = id;
            Name = name;
            Description = description;
            IsUnlocked = isUnlocked;
            IsCleared = isCleared;
        }
    }
}



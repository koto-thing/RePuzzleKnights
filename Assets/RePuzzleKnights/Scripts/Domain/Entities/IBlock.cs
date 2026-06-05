using UnityEngine;

namespace RePuzzleKnights.Scripts.Domain.Entities
{
    public interface IBlock
    {
        string Name { get; }
        Vector3 Position { get; }
    }
}



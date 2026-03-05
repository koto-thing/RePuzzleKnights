using RePuzzleKnights.Scripts.Domain.Entities;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.PathFinder.Block
{
    public class GroundBlock : IBlock
    {
        public string Name { get; }
        public Vector3 Position { get; }

        public GroundBlock(string name, Vector3 position)
        {
            Name = name;
            Position = position;
        }
    }
}


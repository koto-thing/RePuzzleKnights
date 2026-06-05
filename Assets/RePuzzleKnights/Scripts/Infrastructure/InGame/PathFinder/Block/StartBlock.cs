using RePuzzleKnights.Scripts.Domain.Entities;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.PathFinder.Block
{
    public class StartBlock : IBlock
    {
        public string Name { get; }
        public Vector3 Position { get; }

        public StartBlock(string name, Vector3 position)
        {
            Name = name;
            Position = position;
        }
    }
}


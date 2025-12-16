using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.PathFinder.Interface
{
    public interface IBlock
    {
        public string Name { get; }
        public Vector3 Position { get; }
    }
}
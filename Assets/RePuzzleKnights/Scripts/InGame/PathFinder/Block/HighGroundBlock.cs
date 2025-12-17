using UnityEngine;
using RePuzzleKnights.Scripts.InGame.PathFinder.Interface;

namespace RePuzzleKnights.Scripts.InGame.PathFinder.Block
{
    public class HighGroundBlock : IBlock
    {
        private string name;
        private Vector3 position;

        public string Name => name;
        public Vector3 Position => position;

        public HighGroundBlock(string name, Vector3 position)
        {
            this.name = name;
            this.position = position;
        }
    }
}
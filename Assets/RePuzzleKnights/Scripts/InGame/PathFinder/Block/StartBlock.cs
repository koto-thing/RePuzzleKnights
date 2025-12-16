using RePuzzleKnights.Scripts.InGame.PathFinder.Interface;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.PathFinder.Block
{
    public class StartBlock : IBlock
    {
        private string name;
        private Vector3 position;

        public string Name => name;
        public Vector3 Position => position;

        public StartBlock(string name, Vector3 position)
        {
            this.name = name;
            this.position = position;
        }
    }
}
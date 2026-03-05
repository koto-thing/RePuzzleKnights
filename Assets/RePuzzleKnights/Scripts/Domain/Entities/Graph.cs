using System.Collections.Generic;

namespace RePuzzleKnights.Scripts.Domain.Entities
{
    public class Graph
    {
        private readonly Dictionary<string, IBlock> _blocks;
        private readonly Dictionary<string, List<Edge>> _adjacencyList;
        
        public IBlock GetBlock(string name) => _blocks.GetValueOrDefault(name);
        public List<Edge> GetNeighbors(string name) => _adjacencyList.GetValueOrDefault(name);

        public Graph()
        {
            _blocks = new Dictionary<string, IBlock>();
            _adjacencyList = new Dictionary<string, List<Edge>>();
        }
        
        /// <summary>
        /// ブロックを追加
        /// </summary>
        /// <param name="block">追加するブロック</param>
        public void AddBlock(IBlock block)
        {
            if (!_blocks.ContainsKey(block.Name))
            {
                _blocks[block.Name] = block;
                _adjacencyList[block.Name] = new List<Edge>();
            }
        }

        /// <summary>
        /// エッジを追加
        /// </summary>
        /// <param name="from">移動元</param>
        /// <param name="to">移動先</param>
        /// <param name="weight">重み</param>
        public void AddEdge(string from, string to, int weight)
        {
            if (_blocks.ContainsKey(from) && _blocks.ContainsKey(to))
            {
                _adjacencyList[from].Add(new Edge(to, weight));
                _adjacencyList[to].Add(new Edge(from, weight));
            }
        }
    }
}



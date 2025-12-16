using System.Collections.Generic;
using RePuzzleKnights.Scripts.InGame.PathFinder.Interface;

namespace RePuzzleKnights.Scripts.InGame.PathFinder
{
    public class Graph
    {
        private readonly Dictionary<string, IBlock> blocks;
        private readonly Dictionary<string, List<Edge>> adjacencyList;
        
        public IBlock GetBlock(string name) => blocks.GetValueOrDefault(name);
        public List<Edge> GetNeighbors(string name) => adjacencyList.GetValueOrDefault(name);

        public Graph()
        {
            blocks = new Dictionary<string, IBlock>();
            adjacencyList = new Dictionary<string, List<Edge>>();
        }
        
        public void AddBlock(IBlock block)
        {
            if (!blocks.ContainsKey(block.Name))
            {
                blocks[block.Name] = block;
                adjacencyList[block.Name] = new List<Edge>();
            }
        }

        public void AddEdge(string from, string to, int weight)
        {
            if (blocks.ContainsKey(from) && blocks.ContainsKey(to))
            {
                adjacencyList[from].Add(new Edge(to, weight));
                adjacencyList[to].Add(new Edge(from, weight));
            }
        }
    }
}
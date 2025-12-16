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
        
        /// <summary>
        /// Adds a block to the graph. If the block does not already exist in the graph,
        /// it is added along with an empty adjacency list for future edges.
        /// </summary>
        /// <param name="block">The block to add to the graph. Must implement the IBlock interface.</param>
        public void AddBlock(IBlock block)
        {
            if (!blocks.ContainsKey(block.Name))
            {
                blocks[block.Name] = block;
                adjacencyList[block.Name] = new List<Edge>();
            }
        }

        /// <summary>
        /// Adds an edge between two blocks in the graph with a specified weight.
        /// An edge represents a connection between two blocks and is bidirectional,
        /// meaning it will be added in both directions.
        /// </summary>
        /// <param name="from">The name of the starting block.</param>
        /// <param name="to">The name of the destination block.</param>
        /// <param name="weight">The weight of the edge, representing the cost or distance of the connection.</param>
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
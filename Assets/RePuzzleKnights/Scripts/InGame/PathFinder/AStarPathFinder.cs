using System;
using System.Collections.Generic;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.PathFinder
{
    public class AStarPathFinder
    {
        private readonly Graph graph;

        public AStarPathFinder(Graph graph)
        {
            this.graph = graph;
        }

        /// <summary>
        /// Calculates the heuristic value, which estimates the cost to reach the goal block
        /// from the current block in the graph, using the Euclidean distance between their positions.
        /// </summary>
        /// <param name="from">The name of the starting block.</param>
        /// <param name="to">The name of the goal block.</param>
        /// <returns>The estimated cost (heuristic value) as a float.</returns>
        private float Heuristic(string from, string to)
        {
            var blockFrom = graph.GetBlock(from);
            var blockTo = graph.GetBlock(to);
            return Math.Abs(Vector3.Distance(blockFrom.Position, blockTo.Position));
        }

        /// <summary>
        /// Finds the shortest path between the specified start and goal blocks in the graph
        /// using the A* pathfinding algorithm.
        /// </summary>
        /// <param name="startName">The name of the starting block in the graph.</param>
        /// <param name="goalName">The name of the goal block in the graph.</param>
        /// <returns>A tuple containing a list of block names representing the shortest path
        /// and the total cost of the path. Returns null and a cost of -1 if no valid path is found.</returns>
        public (List<string>, float cost) FindPath(string startName, string goalName)
        {
            var startBlock = graph.GetBlock(startName);
            var goalBlock = graph.GetBlock(goalName);

            if (startBlock == null || goalBlock == null)
                return (null, -1);

            var openSet = new PriorityQueue<string, float>();
            var cameFrom = new Dictionary<string, string>();
            var goalCost = new Dictionary<string, float> { [startName] = 0 };
            var heuristicCost = new Dictionary<string, float> { [startName] = Heuristic(startName, goalName) };

            openSet.Enqueue(startName, heuristicCost[startName]);

            while (openSet.Count > 0)
            {
                string current = openSet.Dequeue();

                if (current == goalName)
                    return ReconstructPath(cameFrom, current, goalCost[current]);

                foreach (var edge in graph.GetNeighbors(current))
                {
                    var neighbor = edge.To;
                    var tentativeGoalCost = goalCost[current] + edge.Weight;

                    if (!goalCost.ContainsKey(neighbor) || tentativeGoalCost < goalCost[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        goalCost[neighbor] = tentativeGoalCost;
                        heuristicCost[neighbor] = tentativeGoalCost + Heuristic(neighbor, goalName);

                        openSet.Enqueue(neighbor, heuristicCost[neighbor]);
                    }
                }
            }

            return (null, -1);
        }

        /// <summary>
        /// Reconstructs the shortest path by backtracking from the goal node to the start node,
        /// using the provided "came from" map that tracks the traversal order during pathfinding.
        /// </summary>
        /// <param name="cameFrom">A dictionary that maps each block name to the block name it came from during traversal.</param>
        /// <param name="current">The name of the goal block where the backtracking starts.</param>
        /// <param name="cost">The total cost of the reconstructed path.</param>
        /// <returns>A tuple containing a list of block names representing the reconstructed path and its total cost.</returns>
        private (List<string> path, float cost) ReconstructPath(Dictionary<string, string> cameFrom, string current,
            float cost)
        {
            var totalPath = new List<string> { current };

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Insert(0, current);
            }

            return (totalPath, cost);
        }
    }
}
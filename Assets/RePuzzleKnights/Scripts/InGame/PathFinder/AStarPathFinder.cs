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

        private float Heuristic(string from, string to)
        {
            var blockFrom = graph.GetBlock(from);
            var blockTo = graph.GetBlock(to);
            return Math.Abs(Vector3.Distance(blockFrom.Position, blockTo.Position));
        }

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
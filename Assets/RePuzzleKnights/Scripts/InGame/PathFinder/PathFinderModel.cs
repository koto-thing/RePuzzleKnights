using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.InGame.PathFinder
{
    public class PathFinderModel
    {
        private Graph graph;
        private AStarPathFinder pathFinder;
        
        private string startBlockName;
        private string goalBlockName;

        /// <summary>
        /// Updates the graph instance and relevant properties required for pathfinding.
        /// Initializes a new instance of the A* pathfinder with the updated graph.
        /// </summary>
        /// <param name="graph">The graph instance representing the current state of the pathfinding grid or network.</param>
        /// <param name="startBlockName">The name of the block where the pathfinding operation should start.</param>
        /// <param name="goalBlockName">The name of the block that represents the end goal for the pathfinding operation.</param>
        /// <param name="pathFinder">An instance of the A* pathfinding algorithm to be associated with the updated graph.</param>
        public void UpdateGraph(Graph graph, string startBlockName, string goalBlockName, AStarPathFinder pathFinder)
        {
            this.graph = graph;
            this.startBlockName = startBlockName;
            this.goalBlockName = goalBlockName;

            this.pathFinder = new AStarPathFinder(graph);
        }

        /// <summary>
        /// Finds the shortest path between the start and goal blocks using the A* pathfinding algorithm.
        /// Logs the result of the pathfinding operation, including the path and its cost if a valid path is found,
        /// or a message indicating that no path could be determined.
        /// </summary>
        public void FindPath()
        {
            if (pathFinder == null)
                return;

            Debug.Log($"startBlockName: {startBlockName}, goalBlockName: {goalBlockName}");
            var (pathResult, cost) = pathFinder.FindPath(startBlockName, goalBlockName);

            if (pathResult != null && pathResult.Count > 0)
            {
                Debug.Log($"経路が見つかりました。コスト：{cost}");
                Debug.Log("経路: " + string.Join(" -> ", pathResult));
            }
            else 
            {
                Debug.Log("経路が見つかりませんでした。");
            }
        }
    }
}
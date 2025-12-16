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
        
        public void UpdateGraph(Graph graph, string startBlockName, string goalBlockName, AStarPathFinder pathFinder)
        {
            this.graph = graph;
            this.startBlockName = startBlockName;
            this.goalBlockName = goalBlockName;

            this.pathFinder = new AStarPathFinder(graph);
        }

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
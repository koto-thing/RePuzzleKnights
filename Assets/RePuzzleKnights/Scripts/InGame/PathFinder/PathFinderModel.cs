using System.Collections.Generic;
using UnityEngine;

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

        // void から List<string> を返すように変更
        public List<string> FindPath()
        {
            if (pathFinder == null)
                return null;

            Debug.Log($"Finding Path: {startBlockName} -> {goalBlockName}");
            var (pathResult, cost) = pathFinder.FindPath(startBlockName, goalBlockName);

            if (pathResult != null && pathResult.Count > 0)
            {
                Debug.Log($"経路が見つかりました。コスト：{cost}");
                // 結果を返す
                return pathResult;
            }
            else 
            {
                Debug.Log("経路が見つかりませんでした。");
                return null;
            }
        }
    }
}
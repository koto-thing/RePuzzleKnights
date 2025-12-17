using System;
using R3;
using UnityEngine; // Debug用
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.InGame.PathFinder
{
    public class PathFinderController : IInitializable, IDisposable
    {
        private PathFinderModel model;
        private PathFinderView view;
        private GraphCreator graphCreator;
        private AStarPathFinder pathFinder;
        
        private CompositeDisposable disposables = new ();

        public PathFinderController(PathFinderModel model, PathFinderView view, GraphCreator graphCreator, AStarPathFinder pathFinder)
        {
            this.model = model;
            this.view = view;
            this.graphCreator = graphCreator;
            this.pathFinder = pathFinder;
        }
        
        public void Initialize()
        {
            graphCreator.EnsureGraphReady();

            if (graphCreator.StartBlockNames.Count == 0 || graphCreator.GoalBlockNames.Count == 0)
            {
                Debug.LogWarning("PathFinderController: スタート地点またはゴール地点が見つかりません。");
                return;
            }
            
            string startName = graphCreator.StartBlockNames[0];
            string goalName = graphCreator.GoalBlockNames[0];

            model.UpdateGraph(graphCreator.CreatedGraph, startName, goalName, pathFinder);
            
            // 経路を計算
            var path = model.FindPath();
            if (path != null)
            {
                view.SetCalculatedPath(path);
            }
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
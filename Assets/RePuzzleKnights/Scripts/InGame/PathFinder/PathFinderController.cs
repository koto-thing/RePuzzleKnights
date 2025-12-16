using System;
using R3;
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
            model.UpdateGraph(graphCreator.CreatedGraph, graphCreator.StartBlockName, graphCreator.GoalBlockName, pathFinder);
            model.FindPath();
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
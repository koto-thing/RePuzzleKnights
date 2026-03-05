using System;
using R3;
using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Services;
using RePuzzleKnights.Scripts.Infrastructure.InGame.PathFinder;
using UnityEngine;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    public class PathFinderController : IInitializable, IDisposable
    {
        private readonly FindPathUseCase _findPathUseCase;
        private readonly PathFinderView _view;
        private readonly GraphCreator _graphCreator;
        private readonly IPathFinderService _pathFinderService;
        
        private CompositeDisposable _disposables = new ();

        public PathFinderController(
            FindPathUseCase findPathUseCase, 
            PathFinderView view, 
            GraphCreator graphCreator, 
            IPathFinderService pathFinderService)
        {
            this._findPathUseCase = findPathUseCase;
            this._view = view;
            this._graphCreator = graphCreator;
            this._pathFinderService = pathFinderService;
        }
        
        public void Initialize()
        {
            _graphCreator.EnsureGraphReady();

            if (_graphCreator.StartBlockNames.Count == 0 || _graphCreator.GoalBlockNames.Count == 0)
            {
                Debug.LogWarning("PathFinderController: スタート地点またはゴール地点が見つかりません。");
                return;
            }
            
            _pathFinderService.InitializeGraph(_graphCreator.CreatedGraph, _graphCreator.StartBlockNames, _graphCreator.AllBlockNames);
            
            string startName = _graphCreator.StartBlockNames[0];
            string goalName = _graphCreator.GoalBlockNames[0];
            
            var path = _findPathUseCase.Execute(startName, goalName);
            if (path != null)
            {
                _view.SetCalculatedPath(path);
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}



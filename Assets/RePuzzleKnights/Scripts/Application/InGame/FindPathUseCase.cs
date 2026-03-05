using System.Collections.Generic;
using RePuzzleKnights.Scripts.Domain.Services;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Application.InGame
{
    public class FindPathUseCase
    {
        private readonly IPathFinderService _pathFinderService;

        public FindPathUseCase(IPathFinderService pathFinderService)
        {
            this._pathFinderService = pathFinderService;
        }

        public List<string> Execute(string startBlockName, string goalBlockName)
        {
            Debug.Log($"Finding Path: {startBlockName} -> {goalBlockName}");
            var (pathResult, cost) = _pathFinderService.FindPath(startBlockName, goalBlockName);

            if (pathResult != null && pathResult.Count > 0)
            {
                 Debug.Log($"Path found. Cost: {cost}");
                 return pathResult;
            }
            
            Debug.Log("Path not found.");
            return null;
        }
    }
}



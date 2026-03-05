using System.Collections.Generic;

namespace RePuzzleKnights.Scripts.Domain.Services
{
    public interface IPathFinderService
    {
        (List<string> path, float cost) FindPath(string startName, string goalName);
        void InitializeGraph(Entities.Graph graph, List<string> startBlockNames, List<string> allBlockNames);
    }
}



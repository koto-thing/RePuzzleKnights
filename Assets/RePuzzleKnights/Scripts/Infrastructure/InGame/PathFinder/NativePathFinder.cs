using System.Collections.Generic;
using System.Runtime.InteropServices;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Domain.Services;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.PathFinder
{
    public class NativePathFinder : IPathFinderService
    {
        [DllImport("native_pathfinder")]
        private static extern int find_path(
            Vector3[] nodes,
            int nodeCount,
            int[] edges,
            int edgeCount,
            int startIndex,
            int goalIndex,
            int[] outPath,
            int maxPathLen
        );
        
        private Dictionary<string, int> _nameToIndex = new Dictionary<string, int>();
        private Dictionary<int, string> _indexToName = new Dictionary<int, string>();
        
        private Vector3[] _cachedNodes;
        private int[] _cachedEdges;
        private int _edgeCount;

        public void InitializeGraph(Graph graph, List<string> startBlockNames, List<string> allBlockNames)
        {
            _nameToIndex.Clear();
            _indexToName.Clear();
            
            int count = allBlockNames.Count;
            _cachedNodes = new Vector3[count];

            for (int i = 0; i < count; i++)
            {
                string name = allBlockNames[i];
                var block = graph.GetBlock(name);

                _nameToIndex[name] = i;
                _indexToName[i] = name;
                _cachedNodes[i] = block.Position;
            }
            
            List<int> edgeList = new List<int>();

            foreach (var name in allBlockNames)
            {
                int fromIdx = _nameToIndex[name];
                var neighbors = graph.GetNeighbors(name);

                if (neighbors is null)
                    continue;

                foreach (var neighbor in neighbors)
                {
                    if (_nameToIndex.TryGetValue(neighbor.To, out int toIdx))
                    {
                        edgeList.Add(fromIdx);
                        edgeList.Add(toIdx);
                        edgeList.Add(neighbor.Weight);
                    }
                }
            }

            _cachedEdges = edgeList.ToArray();
            _edgeCount = edgeList.Count / 3;
        }

        public (List<string> path, float cost) FindPath(string startName, string goalName)
        {
            if (!_nameToIndex.ContainsKey(startName) || !_nameToIndex.ContainsKey(goalName))
            {
                Debug.LogError("Start or goal block name not found in the graph.");
                return (null, -1);
            }

            int startIdx = _nameToIndex[startName];
            int goalIdx = _nameToIndex[goalName];
            
            int[] resultBuffer = new int[1024];
            
            int pathLength = find_path(
                _cachedNodes,
                _cachedNodes.Length,
                _cachedEdges,
                _edgeCount,
                startIdx,
                goalIdx,
                resultBuffer,
                resultBuffer.Length
            );

            if (pathLength < 0)
                return (null, -1);

            List<string> pathNames = new List<string>();
            float totalCost = 0f;
            
            for (int i = 0; i < pathLength; i++)
            {
                pathNames.Add(_indexToName[resultBuffer[i]]);
                
                if (i > 0)
                {
                    Vector3 from = _cachedNodes[resultBuffer[i - 1]];
                    Vector3 to = _cachedNodes[resultBuffer[i]];
                    totalCost += Vector3.Distance(from, to);
                }
            }

            return (pathNames, totalCost);
        }
    }
}



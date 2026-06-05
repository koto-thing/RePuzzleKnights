using System.Collections.Generic;
using System.Reflection;
using RePuzzleKnights.Scripts.Domain.Entities;
using UnityEditor;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.PathFinder
{
    public class PathFinderView : MonoBehaviour
    {
        [Header("Graph Gizmos Settings")]
        [SerializeField] private Color nodeColor = Color.yellow;
        [SerializeField] private Color edgeColor = Color.cyan;
        [SerializeField] private float nodeSphereRadius = 0.12f;
        
        [Header("Path Gizmos Settings")]
        [SerializeField] private Color startNodeColor = Color.green;
        [SerializeField] private Color goalNodeColor = Color.red;
        [SerializeField] private Color resultPathColor = Color.magenta;
        [SerializeField] private bool showPathLine = true;
        
        [SerializeField, Range(1f, 20f)] private float pathThickness = 5.0f;

        [SerializeField] private GraphCreator graphCreator;

        private List<string> _currentPath;

        private void Awake()
        {
            if (graphCreator == null)
                graphCreator = GetComponent<GraphCreator>();
        }

        public void SetCalculatedPath(List<string> path)
        {
            this._currentPath = path;
        }

        public void OnDrawGizmos()
        {
            if (graphCreator == null) return;
            var graph = graphCreator.CreatedGraph;
            if (graph == null) return;
            
            DrawGraphStructure(graph);
            
            if (showPathLine && _currentPath != null && _currentPath.Count > 1)
            {
#if UNITY_EDITOR
                DrawThickPath(graph);
#else
                Gizmos.color = resultPathColor;
                for (int i = 0; i < currentPath.Count - 1; i++)
                {
                    var p1 = graph.GetBlock(currentPath[i]).Position + Vector3.up * 0.2f;
                    var p2 = graph.GetBlock(currentPath[i+1]).Position + Vector3.up * 0.2f;
                    Gizmos.DrawLine(p1, p2);
                }
#endif
            }
        }

        /// <summary>
        /// Draws the structure of the graph during the Gizmos drawing phase.
        /// </summary>
        /// <param name="graph">The graph whose structure will be visualized.</param>
        private void DrawGraphStructure(Graph graph)
        {
            // Note: Graph adjacencyList is private. Existing code used reflection.
            // We should ensure the field name matches. In new Domain Graph it is 'adjacencyList'.
            var adjacencyField = typeof(Graph).GetField("adjacencyList", BindingFlags.NonPublic | BindingFlags.Instance);
            if (adjacencyField == null) return;
            var adjacency = adjacencyField.GetValue(graph) as Dictionary<string, List<Edge>>;
            if (adjacency == null) return;

            var drawnEdges = new HashSet<string>();

            foreach (var kvp in adjacency)
            {
                var blockName = kvp.Key;
                var block = graph.GetBlock(blockName);
                if (block == null) continue;

                if (graphCreator.StartBlockNames.Contains(blockName))
                {
                    Gizmos.color = startNodeColor;
                    Gizmos.DrawSphere(block.Position, nodeSphereRadius * 1.5f);
                }
                else if (graphCreator.GoalBlockNames.Contains(blockName))
                {
                    Gizmos.color = goalNodeColor;
                    Gizmos.DrawSphere(block.Position, nodeSphereRadius * 1.5f);
                }
                else
                {
                    Gizmos.color = nodeColor;
                    Gizmos.DrawSphere(block.Position, nodeSphereRadius);
                }

                foreach (var edge in kvp.Value)
                {
                    var to = edge.To;
                    string key = string.Compare(blockName, to, System.StringComparison.Ordinal) < 0 ? blockName + "|" + to : to + "|" + blockName;
                    if (drawnEdges.Contains(key)) continue;

                    var toBlock = graph.GetBlock(to);
                    if (toBlock == null) continue;

                    Gizmos.color = edgeColor;
                    Gizmos.DrawLine(block.Position, toBlock.Position);
                    drawnEdges.Add(key);
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// 経路を太い線で描画する
        /// </summary>
        /// <param name="graph"></param>
        private void DrawThickPath(Graph graph)
        {
            Handles.color = resultPathColor;
            Vector3 offset = Vector3.up * 0.2f;

            for (int i = 0; i < _currentPath.Count - 1; i++)
            {
                var currentName = _currentPath[i];
                var nextName = _currentPath[i + 1];

                var currentBlock = graph.GetBlock(currentName);
                var nextBlock = graph.GetBlock(nextName);

                if (currentBlock != null && nextBlock != null)
                {
                    Vector3 p1 = currentBlock.Position + offset;
                    Vector3 p2 = nextBlock.Position + offset;
                    
                    Handles.DrawAAPolyLine(pathThickness, p1, p2);
                }
            }
        }
#endif
    }
}



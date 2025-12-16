using System.Reflection;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.PathFinder
{
    public class PathFinderView : MonoBehaviour
    {
        [Header("Gizmos Settings")]
        [SerializeField] private Color nodeColor = Color.yellow;
        [SerializeField] private Color edgeColor = Color.cyan;
        [SerializeField] private float nodeSphereRadius = 0.12f;

        private GraphCreator graphCreator;

        public PathFinderView(GraphCreator graphCreator)
        {
            this.graphCreator = graphCreator;
        }

        /// <summary>
        /// 計算した経路をGizmosで表示する
        /// </summary>
        public void OnDrawGizmos()
        {
            // GraphCreator コンポーネントを取得
            if (graphCreator == null)
                return;

            // 作成されたグラフを取得
            var graph = graphCreator.CreatedGraph;
            if (graph == null)
                return;

            // adjacencyList は Graph の private フィールドなのでリフレクションで取得する
            var adjacencyField = typeof(Graph).GetField("adjacencyList", BindingFlags.NonPublic | BindingFlags.Instance);
            if (adjacencyField == null)
                return;

            // adjacencyList を取得
            var adjacency = adjacencyField.GetValue(graph) as System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Edge>>;
            if (adjacency == null)
                return;

            // 描画済みのエッジを管理するセット
            var drawn = new System.Collections.Generic.HashSet<string>();

            // グラフのノードとエッジを描画
            foreach (var kvp in adjacency)
            {
                var blockName = kvp.Key;
                var block = graph.GetBlock(blockName);
                if (block == null)
                    continue;

                // ノードを描画
                Gizmos.color = nodeColor;
                Gizmos.DrawSphere(block.Position, nodeSphereRadius);

                foreach (var edge in kvp.Value)
                {
                    var to = edge.To;
                    string key = string.Compare(blockName, to, System.StringComparison.Ordinal) < 0 ? blockName + "|" + to : to + "|" + blockName;
                    if (drawn.Contains(key))
                        continue;

                    var toBlock = graph.GetBlock(to);
                    if (toBlock == null)
                        continue;

                    Gizmos.color = edgeColor;
                    Gizmos.DrawLine(block.Position, toBlock.Position);
                    drawn.Add(key);
                }
            }
        }
    }
}
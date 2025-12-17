using System;
using System.Collections.Generic;
using RePuzzleKnights.Scripts.InGame.PathFinder.Block;
using RePuzzleKnights.Scripts.InGame.PathFinder.Interface;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.PathFinder
{
    public class GraphCreator : MonoBehaviour
    {
        [Header("Graph Creator Settings")]
        [SerializeField] private List<GameObject> blockGameObjects;

        private Graph graph;
        private bool graphBuilt;

        public Graph CreatedGraph => graph;
        public List<string> StartBlockNames { get; private set; } = new List<string>();
        public List<string> GoalBlockNames { get; private set; } = new List<string>();

        private void Awake()
        {
            EnsureGraphReady();
        }

        /// <summary>
        /// Graph の生成を一度だけ実行する
        /// </summary>
        public void EnsureGraphReady()
        {
            if (graphBuilt)
                return;

            graph = new Graph();
            StartBlockNames.Clear();
            GoalBlockNames.Clear();

            if (blockGameObjects == null || blockGameObjects.Count == 0)
            {
                Debug.LogError("Graph Creator: No block GameObjects assigned.");
                return;
            }

            foreach (var blockObj in blockGameObjects)
            {
                if (blockObj == null)
                {
                    Debug.LogError("Graph Creator: One of the block GameObjects is null.");
                    continue;
                }

                IBlock block;
                var position = blockObj.transform.position;
                var name = blockObj.name;

                switch (blockObj.tag)
                {
                    case "START_BLOCK":
                        block = new StartBlock(name, position);
                        StartBlockNames.Add(name);
                        break;

                    case "GOAL_BLOCK":
                        block = new GoalBlock(name, position);
                        GoalBlockNames.Add(name);
                        break;

                    case "GROUND_BLOCK":
                        block = new GroundBlock(name, position);
                        break;
                    
                    case "HIGHGROUND_BLOCK":
                        block = new HighGroundBlock(name, position);
                        break;

                    default:
                        Debug.LogError($"GraphCreator: GameObject '{name}' には未処理のタグ '{blockObj.tag}' が付いています。");
                        continue;
                }

                graph.AddBlock(block);
            }

            SetupEdges();
            graphBuilt = true;
        }

        /// <summary>
        /// 指定した座標に最も近いブロックの名前を取得する
        /// </summary>
        /// <param name="position">指定座標</param>
        /// <returns>最も近いブロックの名前</returns>
        public string GetNearestBlockName(Vector3 position)
        {
            if (graph == null)
                return null;

            string nearestBlockName = null;
            float minDistanceSqr = float.MaxValue;

            foreach (var blockObj in blockGameObjects)
            {
                if (blockObj == null)
                    continue;

                float distSqr = (blockObj.transform.position - position).sqrMagnitude;
                if (distSqr < minDistanceSqr)
                {
                    minDistanceSqr = distSqr;
                    nearestBlockName = blockObj.name;
                }
            }

            return nearestBlockName;
        }

        /// <summary>
        /// Establishes edges between blocks in the graph based on their spatial proximity.
        /// Only blocks that are within a specified unit distance (squared) are connected.
        /// </summary>
        private void SetupEdges()
        {
            if (graph == null)
                return;

            List<IBlock> blocksInGraph = new List<IBlock>();
            foreach (var blockObj in blockGameObjects)
            {
                if (blockObj == null)
                    continue;

                var block = graph.GetBlock(blockObj.name);
                blocksInGraph.Add(block);
            }

            if (blocksInGraph.Count < 2)
                return;

            // 許容範囲を設定
            float threshold = 0.1f;
            
            for (int i = 0; i < blocksInGraph.Count; i++)
            {
                var blockA = blocksInGraph[i];
                for (int j = i + 1; j < blocksInGraph.Count; j++)
                {
                    var blockB = blocksInGraph[j];
                    float distanceSquared = (blockA.Position - blockB.Position).sqrMagnitude;
                    if (Mathf.Abs(distanceSquared - 1.0f) < threshold)
                    {
                        graph.AddEdge(blockA.Name, blockB.Name, 1);
                    }
                }
            }
        }
    }
}

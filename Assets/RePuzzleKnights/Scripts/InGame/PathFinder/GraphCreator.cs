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
        public string StartBlockName { get; private set; }
        public string GoalBlockName { get; private set; }

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
                        StartBlockName = name;
                        break;

                    case "GOAL_BLOCK":
                        block = new GoalBlock(name, position);
                        GoalBlockName = name;
                        break;

                    case "NORMAL_BLOCK":
                        block = new NormalBlock(name, position);
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

            for (int i = 0; i < blocksInGraph.Count; i++)
            {
                var blockA = blocksInGraph[i];
                for (int j = i + 1; j < blocksInGraph.Count; j++)
                {
                    var blockB = blocksInGraph[j];
                    float distanceSquared = (blockA.Position - blockB.Position).sqrMagnitude;
                    if (Mathf.Approximately(distanceSquared, 1.0f * 1.0f))
                        graph.AddEdge(blockA.Name, blockB.Name, 1);
                }
            }
        }
    }
}

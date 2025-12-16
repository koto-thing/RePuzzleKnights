using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RePuzzleKnights.Scripts.InGame.Enemies.BugEnemy;
using RePuzzleKnights.Scripts.InGame.PathFinder;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    public class EnemyFactory
    {
        private readonly GraphCreator graphCreator;

        public EnemyFactory(GraphCreator graphCreator)
        {
            this.graphCreator = graphCreator;
        }

        /// <summary>
        /// Asynchronously creates an instance of a bug enemy and initializes its movement controller.
        /// </summary>
        /// <param name="prefabRef">A reference to the bug enemy prefab asset.</param>
        /// <param name="data">The data associated with the bug enemy, including attributes and waypoints.</param>
        /// <param name="spawnPosition">The position at which the bug enemy will be spawned.</param>
        /// <returns>Returns an instance of <see cref="BugEnemyMoveController"/> that manages the spawned enemy.</returns>
        public async UniTask<BugEnemyMoveController> CreateAsync(
            AssetReferenceGameObject prefabRef,
            BugEnemyDataSO data,
            Vector3 spawnPosition)
        {
            // オブジェクトを生成する
            GameObject instanceObj = await Addressables.InstantiateAsync(
                prefabRef, 
                spawnPosition, 
                Quaternion.identity).ToUniTask();

            if (!instanceObj.TryGetComponent<BugEnemyView>(out var view))
            {
                Debug.LogError($"Spawned object {instanceObj.name} does not have BugEnemyView component!");
                return null;
            }

            // 経路の決定ロジック
            List<Vector3> wayPointsToUse = new List<Vector3>();

            if (data.WayPoints != null && data.WayPoints.Count > 0)
            {
                // SOに設定があればそれを使う
                wayPointsToUse = new List<Vector3>(data.WayPoints);
            }
            else
            {
                // 設定がなければ経路探索を使用
                wayPointsToUse = CalculatePathFromGraph();
            }
            
            var model = new BugEnemyMoveModel();
            var controller = new BugEnemyMoveController(model, view, data, wayPointsToUse);

            controller.Initialize();

            return controller;
        }

        /// <summary>
        /// グラフから最短経路を計算する
        /// </summary>
        /// <returns></returns>
        private List<Vector3> CalculatePathFromGraph()
        {
            if (graphCreator == null)
            {
                Debug.LogWarning("EnemyFactory: graphCreator is null");
                return new List<Vector3>();
            }
    
            graphCreator.EnsureGraphReady();

            var graph = graphCreator.CreatedGraph;
            var startName = graphCreator.StartBlockName;
            var goalName = graphCreator.GoalBlockName;

            var pathFinder = new AStarPathFinder(graph);
            var (pathNodeNames, _) = pathFinder.FindPath(startName, goalName);

            if (pathNodeNames == null || pathNodeNames.Count == 0)
            {
                Debug.LogWarning("EnemyFactory: Path not found via Graph.");
                return new List<Vector3>();
            }

            List<Vector3> vectorPath = new List<Vector3>();
            foreach (var nodeName in pathNodeNames)
            {
                var block = graph.GetBlock(nodeName);
                if (block != null)
                    vectorPath.Add(block.Position + new Vector3(0.0f, 0.5f, 0.0f));
            }
            
            return vectorPath;
        }
    }
}
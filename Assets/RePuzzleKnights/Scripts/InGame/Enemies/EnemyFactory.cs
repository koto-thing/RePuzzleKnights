using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RePuzzleKnights.Scripts.InGame.BaseSystem;
using RePuzzleKnights.Scripts.InGame.Enemies.BugEnemy;
using RePuzzleKnights.Scripts.InGame.PathFinder;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    public class EnemyFactory : IEnemyFactoryService
    {
        private readonly GraphCreator graphCreator;
        private readonly BaseStatusModel baseStatusModel;

        public EnemyFactory(GraphCreator graphCreator, BaseStatusModel baseStatusModel)
        {
            this.graphCreator = graphCreator;
            this.baseStatusModel = baseStatusModel;
        }

        public async UniTask<GameObject> CreateEnemyAsync(
            AssetReferenceGameObject prefabRef,
            BugEnemyDataSO data,
            Vector3 spawnPosition)
        {
            GameObject instanceObj = await Addressables.InstantiateAsync(
                prefabRef, 
                spawnPosition, 
                Quaternion.identity).ToUniTask();

            if (!instanceObj.TryGetComponent<BugEnemyView>(out var view))
            {
                Debug.LogError($"Spawned object {instanceObj.name} does not have BugEnemyView component!");
                return null;
            }

            List<Vector3> wayPointsToUse = CalculatePath(spawnPosition, data);
            
            var moveModel = new BugEnemyMoveModel();
            var statusModel = new BugEnemyStatusModel();
            
            var controller = new BugEnemyMoveController(
                moveModel, 
                statusModel, 
                view, 
                data, 
                baseStatusModel, 
                wayPointsToUse);
            
            view.SetController(controller);
            
            controller.Initialize();

            return instanceObj;
        }

        public List<Vector3> CalculatePath(Vector3 spawnPosition, BugEnemyDataSO data)
        {
            if (data.WayPoints != null && data.WayPoints.Count > 0)
            {
                return new List<Vector3>(data.WayPoints);
            }

            return CalculatePathFromGraph(spawnPosition);
        }

        private List<Vector3> CalculatePathFromGraph(Vector3 spawnPosition)
        {
            if (graphCreator == null)
            {
                Debug.LogWarning("EnemyFactory: GraphCreator is null.");
                return new List<Vector3>();
            }

            var graph = graphCreator.CreatedGraph;
            if (graph == null)
            {
                Debug.LogWarning("EnemyFactory: Graph is null.");
                return new List<Vector3>();
            }

            var startName = graphCreator.GetNearestBlockName(spawnPosition);
            if (string.IsNullOrEmpty(startName))
            {
                Debug.LogWarning("EnemyFactory: Could not find nearest block.");
                return new List<Vector3>();
            }

            if (graphCreator.GoalBlockNames.Count == 0)
            {
                Debug.LogWarning("EnemyFactory: Goal block not found.");
                return new List<Vector3>();
            }
            
            int randomIndex = Random.Range(0, graphCreator.GoalBlockNames.Count);
            var goalName = graphCreator.GoalBlockNames[randomIndex];

            var pathFinder = new AStarPathFinder(graph);
            var (pathNodeNames, _) = pathFinder.FindPath(startName, goalName);
            
            if (pathNodeNames == null || pathNodeNames.Count == 0)
            {
                Debug.LogWarning($"EnemyFactory: Path not found from {startName} to {goalName}.");
                return new List<Vector3>();
            }

            List<Vector3> vectorPath = new List<Vector3>();
            foreach (var nodeName in pathNodeNames)
            {
                var block = graph.GetBlock(nodeName);
                if (block != null)
                {
                    vectorPath.Add(block.Position + new Vector3(0, 0.5f, 0));
                }
            }

            return vectorPath;
        }
    }
}


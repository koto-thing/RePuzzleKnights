using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RePuzzleKnights.Scripts.InGame.BaseSystem;
using RePuzzleKnights.Scripts.InGame.Enemies.Interface;
using RePuzzleKnights.Scripts.InGame.Enemies.SO;
using RePuzzleKnights.Scripts.InGame.PathFinder;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    /// <summary>
    /// 敵キャラクターの生成を管理するファクトリークラス
    /// Addressablesを使った非同期生成、経路計算、MVC構造の初期化を担当
    /// </summary>
    public class EnemyFactory : IEnemyFactoryService
    {
        private readonly GraphCreator graphCreator;
        private readonly BaseStatusModel baseStatusModel;

        public EnemyFactory(GraphCreator graphCreator, BaseStatusModel baseStatusModel)
        {
            this.graphCreator = graphCreator;
            this.baseStatusModel = baseStatusModel;
        }

        /// <summary>
        /// 敵キャラクターを非同期で生成
        /// </summary>
        public async UniTask<GameObject> CreateEnemyAsync(
            AssetReferenceGameObject prefabRef,
            EnemyDataSO data,
            Vector3 spawnPosition)
        {
            // プレハブの生成
            var prefab = data.PrefabRef;
            if (prefabRef != null && prefabRef.RuntimeKeyIsValid()) 
                prefab = prefabRef;

            GameObject instance = await Addressables.InstantiateAsync(prefab, spawnPosition, Quaternion.identity);
            var view = instance.GetComponent<EnemyView>();

            // 経路計算
            List<Vector3> path = CalculatePath(spawnPosition, data);

            // MVC構造の構築
            var model = new EnemyModel(data, path);
            var controller = new EnemyController(model, instance.transform);
            var presenter = new EnemyPresenter(model, controller, view, baseStatusModel);
    
            // 初期化
            presenter.Initialize();

            return instance;
        }
        
        /// <summary>
        /// 敵の移動経路を計算
        /// </summary>
        public List<Vector3> CalculatePath(Vector3 spawnPosition, EnemyDataSO data)
        {
            return CalculatePathFromGraph(spawnPosition);
        }

        /// <summary>
        /// グラフデータから経路を計算
        /// </summary>
        private List<Vector3> CalculatePathFromGraph(Vector3 spawnPosition)
        {
            if (graphCreator == null) 
                return new List<Vector3>();
            
            var graph = graphCreator.CreatedGraph;
            if (graph == null) 
                return new List<Vector3>();

            var startName = graphCreator.GetNearestBlockName(spawnPosition);
            if (string.IsNullOrEmpty(startName)) 
                return new List<Vector3>();
            
            if (graphCreator.GoalBlockNames.Count == 0) 
                return new List<Vector3>();
            
            int randomIndex = Random.Range(0, graphCreator.GoalBlockNames.Count);
            var goalName = graphCreator.GoalBlockNames[randomIndex];

            var pathFinder = new AStarPathFinder(graph);
            var (pathNodeNames, _) = pathFinder.FindPath(startName, goalName);
            
            if (pathNodeNames == null || pathNodeNames.Count == 0)
                return new List<Vector3>();

            List<Vector3> vectorPath = new List<Vector3>();
            foreach (var nodeName in pathNodeNames)
            {
                var block = graph.GetBlock(nodeName);
                if (block != null) 
                    vectorPath.Add(block.Position + new Vector3(0, 0.5f, 0));
            }

            return vectorPath;
        }
    }
}


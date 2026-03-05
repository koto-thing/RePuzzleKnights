using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Enemies.Definitions;
using RePuzzleKnights.Scripts.Infrastructure.InGame.PathFinder;
using RePuzzleKnights.Scripts.Presentation.InGame;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame
{
    public class EnemyFactory
    {
        private readonly IObjectResolver _container;
        private readonly GraphCreator _graphCreator;
        private readonly FindPathUseCase _findPathUseCase;
        private readonly BaseStatusUseCase _baseStatusUseCase;
        
        private Action _onEnemyDefeated;

        public EnemyFactory(
            IObjectResolver container,
            GraphCreator graphCreator,
            FindPathUseCase findPathUseCase,
            BaseStatusUseCase baseStatusUseCase)
        {
            this._container = container;
            this._graphCreator = graphCreator;
            this._findPathUseCase = findPathUseCase;
            this._baseStatusUseCase = baseStatusUseCase;
        }

        public void SetOnEnemyDefeatedCallback(Action callback)
        {
            _onEnemyDefeated = callback;
        }

        public async UniTask<Enemy> CreateEnemyAsync(EnemyDataSO data, Vector3 spawnPosition = default)
        {
            if (data == null || data.PrefabRef == null) return null;
            
            List<Vector3> path = CalculatePath(spawnPosition, data);
            
            Vector3 startPos = path.Count > 0 ? path[0] : spawnPosition;
            
            var handle = Addressables.InstantiateAsync(data.PrefabRef, startPos, Quaternion.identity);
            var go = await handle.ToUniTask();
            if (go == null) return null;
            
            var stats = new EnemyStats(
                data.EnemyName,
                data.MaxHp,
                data.MoveSpeed,
                data.AttackPower,
                data.AttackInterval,
                data.Rank,
                (int)data.AttackRange,
                data.MoveType,
                data.IgnoreTerrain
            );
            var enemy = new Enemy(System.Guid.NewGuid().ToString(), stats, path, startPos);
            
            var controller = new EnemyController(enemy);
            
            var view = go.GetComponent<EnemyView>();
            if (view == null) view = go.AddComponent<EnemyView>();
            
            view.Construct(controller);
            
            var entityHolder = go.GetComponent<EnemyEntityHolder>();
            if (entityHolder == null) entityHolder = go.AddComponent<EnemyEntityHolder>();
            
            var enemyAdapter = new EnemyAdapter(enemy, view);
            entityHolder.SetEntity(enemyAdapter);
            
            var presenter = new EnemyPresenter(enemy, view, _baseStatusUseCase, _onEnemyDefeated);
            presenter.Initialize();
            
            enemy.StartMoving();

            return enemy;
        }
        
        private List<Vector3> CalculatePath(Vector3 spawnPosition, EnemyDataSO data)
        {
            if (_graphCreator == null || _graphCreator.CreatedGraph == null) 
                return new List<Vector3>();

            var startName = _graphCreator.GetNearestBlockName(spawnPosition);
            if (string.IsNullOrEmpty(startName)) 
                return new List<Vector3>();
            
            if (_graphCreator.GoalBlockNames.Count == 0) 
                return new List<Vector3>();
            
            int randomIndex = UnityEngine.Random.Range(0, _graphCreator.GoalBlockNames.Count);
            var goalName = _graphCreator.GoalBlockNames[randomIndex];

            var pathNodeNames = _findPathUseCase.Execute(startName, goalName);
            
            if (pathNodeNames == null || pathNodeNames.Count == 0) 
                return new List<Vector3>();

            List<Vector3> vectorPath = new List<Vector3>();
            foreach (var nodeName in pathNodeNames)
            {
                var block = _graphCreator.CreatedGraph.GetBlock(nodeName);
                if (block != null) 
                    vectorPath.Add(block.Position + new Vector3(0, 0.5f, 0));
            }
            
            return vectorPath;
        }
    }
}



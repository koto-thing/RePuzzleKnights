using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using RePuzzleKnights.Scripts.Domain.Services;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Enemies.Definitions;
using UnityEngine;
using VContainer;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame
{
    public class EnemySpawner : MonoBehaviour, ILevelStatusProvider
    {
        [SerializeField] private List<EnemyWaveSO> waves;
        [SerializeField] private Transform spawnPoint;

        private EnemyFactory _enemyFactory;
        private int _currentWaveIndex;
        
        private readonly ReactiveProperty<bool> _isAllWavesFinished = new(false);
        public ReadOnlyReactiveProperty<bool> IsAllWavesFinished => _isAllWavesFinished;
        
        private readonly ReactiveProperty<int> _activeEnemyCount = new(0);
        public ReadOnlyReactiveProperty<int> ActiveEnemyCount => _activeEnemyCount;

        [Inject]
        public void Construct(EnemyFactory factory)
        {
            this._enemyFactory = factory;
            this._enemyFactory.SetOnEnemyDefeatedCallback(NotifyEnemyDefeated);
        }

        private void Start()
        {
            StartAllWaves(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid StartAllWaves(CancellationToken token)
        {
            try
            {
                while (_currentWaveIndex < waves.Count)
                {
                    await StartWave(token);
                    _currentWaveIndex++;
                }
                
                Debug.Log("[EnemySpawner] All waves completed.");
                _isAllWavesFinished.Value = true;
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[EnemySpawner] Wave spawning cancelled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EnemySpawner] Error in wave spawning: {ex.Message}");
            }
        }

        private async UniTask StartWave(CancellationToken token)
        {
            if (_currentWaveIndex >= waves.Count) return;

            var currentWave = waves[_currentWaveIndex];
            Debug.Log($"[EnemySpawner] Wave {_currentWaveIndex + 1} Started.");

            var tasks = new List<UniTask>();
            foreach (var entry in currentWave.SpawnEntries)
            {
                tasks.Add(ProcessSpawnEntry(entry, token));
            }

            await UniTask.WhenAll(tasks);
            Debug.Log($"[EnemySpawner] Wave {_currentWaveIndex + 1} Completed.");
        }

        private async UniTask ProcessSpawnEntry(EnemySpawnEntry entry, CancellationToken token)
        {
            if (entry == null)
            {
                Debug.LogError("[EnemySpawner] EnemySpawnEntry is null.");
                return;
            }
            
            if (entry.EnemyDataSO == null)
            {
                Debug.LogError("[EnemySpawner] EnemyDataSO is null in spawn entry.");
                return;
            }
            
            if (entry.InitialDelay > 0)
                await UniTask.Delay(TimeSpan.FromSeconds(entry.InitialDelay), cancellationToken: token);

            for (int i = 0; i < entry.Count; i++)
            {
                Vector3 startPos = spawnPoint != null ? spawnPoint.position + new Vector3(0.0f, 0.5f, 0.0f) : Vector3.zero;
                
                if (entry.EnemyPrefabRef != null && entry.EnemyPrefabRef.RuntimeKeyIsValid())
                {
                    _activeEnemyCount.Value++;
                    await _enemyFactory.CreateEnemyAsync(entry.EnemyDataSO, startPos);
                }
                else
                {
                    Debug.LogWarning("[EnemySpawner] Enemy Prefab Reference is missing or invalid.");
                }

                if (i < entry.Count - 1)
                    await UniTask.Delay(TimeSpan.FromSeconds(entry.Interval), cancellationToken: token);
            }
        }

        public void NotifyEnemyDefeated()
        {
            _activeEnemyCount.Value = Mathf.Max(0, _activeEnemyCount.Value - 1);
            Debug.Log($"[EnemySpawner] Enemy defeated. Active enemies: {_activeEnemyCount.Value}");
        }

        private void OnDestroy()
        {
            _isAllWavesFinished?.Dispose();
            _activeEnemyCount?.Dispose();
        }
    }
}


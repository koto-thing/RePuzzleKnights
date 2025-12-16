using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private List<EnemyWaveSO> waves;
        [SerializeField] private Transform spawnPoint;

        private EnemyFactory enemyFactory;
        private int currentWaveIndex = 0;

        [Inject]
        public void Construct(EnemyFactory enemyFactory)
        {
            this.enemyFactory = enemyFactory;
        }

        private void Start()
        {
            StartWave(this.GetCancellationTokenOnDestroy()).Forget();
        }

        /// <summary>
        /// Initiates the spawning of enemies for the current wave and progresses through all defined waves.
        /// </summary>
        /// <param name="token">A CancellationToken used to monitor for cancellation requests during the enemy spawning process.</param>
        /// <returns>A UniTask representing the asynchronous wave spawning operation.</returns>
        private async UniTask StartWave(CancellationToken token)
        {
            if (currentWaveIndex >= waves.Count)
            {
                Debug.Log("All waves completed.");
                return;
            }

            var currentWave = waves[currentWaveIndex];
            Debug.Log($"Wave {currentWaveIndex + 1} Started.");

            var tasks = new List<UniTask>();
            foreach (var entry in currentWave.SpawnEntries)
            {
                tasks.Add(ProcessSpawnEntry(entry, token));
            }

            await UniTask.WhenAll(tasks);
            Debug.Log($"Wave {currentWaveIndex + 1} Completed.");

            currentWaveIndex++;
        }

        /// <summary>
        /// Processes a single spawn entry by spawning the specified number of enemies at defined intervals.
        /// </summary>
        /// <param name="entry">The spawn entry that defines the enemy type, quantity, spawn interval, and initial delay.</param>
        /// <param name="token">A CancellationToken used to monitor for cancellation requests during the spawning process.</param>
        /// <returns>A UniTask representing the asynchronous spawn entry operation.</returns>
        private async UniTask ProcessSpawnEntry(EnemySpawnEntry entry, CancellationToken token)
        {
            if (entry.InitialDelay > 0)
                await UniTask.Delay(TimeSpan.FromSeconds(entry.InitialDelay), cancellationToken: token);

            for (int i = 0; i < entry.Count; i++)
            {
                Vector3 startPos = spawnPoint != null ? spawnPoint.position + new Vector3(0.0f, 0.5f, 0.0f) : Vector3.zero;
                if (entry.EnemyPrefabRef != null && entry.EnemyPrefabRef.RuntimeKeyIsValid())
                    await enemyFactory.CreateAsync(entry.EnemyPrefabRef, entry.EnemyDataSO, startPos);
                else
                    Debug.LogWarning("Enemy Prefab Reference is missing or invalid.");

                if (i < entry.Count - 1)
                    await UniTask.Delay(TimeSpan.FromSeconds(entry.Interval), cancellationToken: token);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    /// <summary>
    /// 敵のウェーブスポーンを管理するクラス
    /// 指定されたウェーブデータに基づいて、複数の敵を順次生成する
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private List<EnemyWaveSO> waves;
        [SerializeField] private Transform spawnPoint;

        private IEnemyFactoryService enemyFactory;
        private int currentWaveIndex = 0;

        /// <summary>
        /// 依存性注入
        /// </summary>
        [Inject]
        public void Construct(IEnemyFactoryService enemyFactory)
        {
            this.enemyFactory = enemyFactory;
        }

        private void Start()
        {
            StartWave(this.GetCancellationTokenOnDestroy()).Forget();
        }

        /// <summary>
        /// ウェーブを開始し、すべてのウェーブを順次実行
        /// </summary>
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
        /// 単一のスポーンエントリを処理し、指定された間隔で敵を生成
        /// </summary>
        private async UniTask ProcessSpawnEntry(EnemySpawnEntry entry, CancellationToken token)
        {
            if (entry.InitialDelay > 0)
                await UniTask.Delay(TimeSpan.FromSeconds(entry.InitialDelay), cancellationToken: token);

            for (int i = 0; i < entry.Count; i++)
            {
                Vector3 startPos = spawnPoint != null ? spawnPoint.position + new Vector3(0.0f, 0.5f, 0.0f) : Vector3.zero;
                
                if (entry.EnemyPrefabRef != null && entry.EnemyPrefabRef.RuntimeKeyIsValid())
                {
                    await enemyFactory.CreateEnemyAsync(entry.EnemyPrefabRef, entry.EnemyDataSO, startPos);
                }
                else
                {
                    Debug.LogWarning("Enemy Prefab Reference is missing or invalid.");
                }

                if (i < entry.Count - 1)
                    await UniTask.Delay(TimeSpan.FromSeconds(entry.Interval), cancellationToken: token);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using RePuzzleKnights.Scripts.InGame.Enemies.Interface;
using RePuzzleKnights.Scripts.InGame.Enemies.SO;
using RePuzzleKnights.Scripts.InGame.GameFlowSystem.Interface;
using UnityEngine;
using VContainer;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    /// <summary>
    /// 敵のウェーブスポーンを管理するクラス
    /// 指定されたウェーブデータに基づいて、複数の敵を順次生成する
    /// ウェーブ完了状態と敵の数を外部に提供
    /// </summary>
    public class EnemySpawner : MonoBehaviour, IWaveInfoProvider, IEnemyInfoProvider
    {
        [SerializeField] private List<EnemyWaveSO> waves;
        [SerializeField] private Transform spawnPoint;

        private IEnemyFactoryService enemyFactory;
        private int currentWaveIndex;
        
        // ウェーブ完了状態
        private readonly ReactiveProperty<bool> isAllWavesFinished = new(false);
        public ReadOnlyReactiveProperty<bool> IsAllWavesFinished => isAllWavesFinished;
        
        // アクティブな敵の数
        private readonly ReactiveProperty<int> activeEnemyCount = new(0);
        public ReadOnlyReactiveProperty<int> ActiveEnemyCount => activeEnemyCount;

        /// <summary>
        /// 依存性注入
        /// </summary>
        [Inject]
        public void Construct(IEnemyFactoryService factory)
        {
            this.enemyFactory = factory;
            
            // EnemyFactoryの場合、倒された敵の通知コールバックを設定
            if (factory is EnemyFactory enemyFactoryImpl)
            {
                enemyFactoryImpl.SetOnEnemyDefeatedCallback(NotifyEnemyDefeated);
            }
        }

        private void Start()
        {
            StartAllWaves(this.GetCancellationTokenOnDestroy()).Forget();
        }

        /// <summary>
        /// すべてのウェーブを順次実行
        /// </summary>
        private async UniTaskVoid StartAllWaves(CancellationToken token)
        {
            try
            {
                while (currentWaveIndex < waves.Count)
                {
                    await StartWave(token);
                    currentWaveIndex++;
                }
                
                Debug.Log("[EnemySpawner] All waves completed.");
                isAllWavesFinished.Value = true;
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

        /// <summary>
        /// ウェーブを開始し、すべての敵を生成
        /// </summary>
        private async UniTask StartWave(CancellationToken token)
        {
            if (currentWaveIndex >= waves.Count)
                return;

            var currentWave = waves[currentWaveIndex];
            Debug.Log($"[EnemySpawner] Wave {currentWaveIndex + 1} Started.");

            var tasks = new List<UniTask>();
            foreach (var entry in currentWave.SpawnEntries)
            {
                tasks.Add(ProcessSpawnEntry(entry, token));
            }

            await UniTask.WhenAll(tasks);
            Debug.Log($"[EnemySpawner] Wave {currentWaveIndex + 1} Completed.");
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
                    activeEnemyCount.Value++;
                    await enemyFactory.CreateEnemyAsync(entry.EnemyPrefabRef, entry.EnemyDataSO, startPos);
                }
                else
                {
                    Debug.LogWarning("[EnemySpawner] Enemy Prefab Reference is missing or invalid.");
                }

                if (i < entry.Count - 1)
                    await UniTask.Delay(TimeSpan.FromSeconds(entry.Interval), cancellationToken: token);
            }
        }

        /// <summary>
        /// 敵が倒されたときに呼び出される
        /// </summary>
        public void NotifyEnemyDefeated()
        {
            activeEnemyCount.Value = Mathf.Max(0, activeEnemyCount.Value - 1);
            Debug.Log($"[EnemySpawner] Enemy defeated. Active enemies: {activeEnemyCount.Value}");
        }

        private void OnDestroy()
        {
            isAllWavesFinished?.Dispose();
            activeEnemyCount?.Dispose();
        }
    }
}
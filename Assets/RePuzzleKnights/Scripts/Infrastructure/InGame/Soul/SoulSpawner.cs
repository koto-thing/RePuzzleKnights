using System.Collections.Generic;
using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Enums;
using UnityEngine;
using VContainer;
using R3;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.Soul
{
    /// <summary>
    /// ステージ内のブロック上に全属性のSoulをランダムに定期スポーンするクラス。
    /// </summary>
    public class SoulSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private List<GameObject> soulPrefabs = new(); // 属性ごとのプレハブ
        [SerializeField] private float spawnInterval = 3f;
        [SerializeField] private int maxSoulCount = 5;
        [SerializeField] private float spawnHeightOffset = 0.5f;

        [Header("Target Blocks Tags")]
        [SerializeField] private string groundTag = "GROUND_BLOCK";
        [SerializeField] private string highGroundTag = "HIGHGROUND_BLOCK";

        private SoulUseCase _soulUseCase;
        private float _spawnTimer = 0f;
        private readonly List<SoulObjectView> _spawnedSouls = new();
        private readonly List<GameObject> _stageBlocks = new();
        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public void Construct(SoulUseCase soulUseCase)
        {
            _soulUseCase = soulUseCase;
        }

        private void Start()
        {
            FindStageBlocks();
            _spawnTimer = spawnInterval;
        }

        private void Update()
        {
            _spawnedSouls.RemoveAll(item => item == null);

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= spawnInterval)
            {
                _spawnTimer = 0f;
                TrySpawnSoul();
            }
        }

        private void FindStageBlocks()
        {
            _stageBlocks.Clear();
            var grounds = GameObject.FindGameObjectsWithTag(groundTag);
            var highGrounds = GameObject.FindGameObjectsWithTag(highGroundTag);

            _stageBlocks.AddRange(grounds);
            _stageBlocks.AddRange(highGrounds);

            Debug.Log($"[SoulSpawner] Found {_stageBlocks.Count} blocks in the stage (Ground: {grounds.Length}, HighGround: {highGrounds.Length})");
        }

        private void TrySpawnSoul()
        {
            if (_spawnedSouls.Count >= maxSoulCount) return;

            if (_stageBlocks.Count == 0)
            {
                FindStageBlocks();
                if (_stageBlocks.Count == 0)
                {
                    Debug.LogWarning("[SoulSpawner] No blocks found with tags Ground or HighGround.");
                    return;
                }
            }

            GameObject selectedBlock = null;
            Vector3 spawnPosition = Vector3.zero;

            for (int i = 0; i < 10; i++)
            {
                var block = _stageBlocks[Random.Range(0, _stageBlocks.Count)];
                if (block == null) continue;

                Vector3 targetPos = block.transform.position;
                var col = block.GetComponent<Collider>();
                if (col != null)
                {
                    targetPos.y = col.bounds.max.y + spawnHeightOffset;
                }
                else
                {
                    targetPos.y += spawnHeightOffset;
                }

                if (!IsSoulNear(targetPos, 0.4f))
                {
                    selectedBlock = block;
                    spawnPosition = targetPos;
                    break;
                }
            }

            if (selectedBlock != null)
            {
                SpawnSoul(spawnPosition);
            }
        }

        private bool IsSoulNear(Vector3 position, float radius)
        {
            foreach (var soul in _spawnedSouls)
            {
                if (soul != null && Vector3.Distance(soul.transform.position, position) < radius)
                {
                    return true;
                }
            }
            return false;
        }

        private void SpawnSoul(Vector3 position)
        {
            GameObject soulObj;
            SoulObjectView soulView;

            // スポーンさせる属性をランダムに決定 (Fire〜Dark)
            ElementType element = (ElementType)Random.Range((int)ElementType.Fire, (int)ElementType.Dark + 1);

            GameObject targetPrefab = null;
            if (soulPrefabs != null && soulPrefabs.Count > 0)
            {
                // 属性に対応するプレハブを名前（大文字小文字無視）で探す
                string elemStr = element.ToString().ToLower();
                foreach (var p in soulPrefabs)
                {
                    if (p != null && p.name.ToLower().Contains(elemStr))
                    {
                        targetPrefab = p;
                        break;
                    }
                }
                
                // 見つからなければランダムに割り当てる
                if (targetPrefab == null)
                {
                    targetPrefab = soulPrefabs[Random.Range(0, soulPrefabs.Count)];
                }
            }

            if (targetPrefab != null)
            {
                soulObj = Instantiate(targetPrefab, position, Quaternion.identity);
                soulView = soulObj.GetComponent<SoulObjectView>();
                if (soulView == null)
                {
                    soulView = soulObj.AddComponent<SoulObjectView>();
                }
            }
            else
            {
                soulObj = new GameObject($"SoulObject_{element}_Dynamic");
                soulObj.transform.position = position;
                soulView = soulObj.AddComponent<SoulObjectView>();
            }

            // 属性を初期化
            soulView.Initialize(element);

            soulView.OnCollected
                .Subscribe(collectedView =>
                {
                    if (_soulUseCase != null)
                    {
                        _soulUseCase.AddSoul(collectedView.Element, collectedView.SoulValue);
                    }
                })
                .AddTo(_disposables);

            _spawnedSouls.Add(soulView);
            Debug.Log($"[SoulSpawner] Spawned {element} Soul at {position}");
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            foreach (var block in _stageBlocks)
            {
                if (block != null)
                {
                    var col = block.GetComponent<Collider>();
                    Vector3 center = block.transform.position;
                    if (col != null)
                    {
                        center.y = col.bounds.max.y + spawnHeightOffset;
                    }
                    Gizmos.DrawWireSphere(center, 0.2f);
                }
            }
        }
    }
}

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

namespace RePuzzleKnights.Scripts.InGame.Rendering
{
    public class DissolveEffect : MonoBehaviour
    {
        [Header("基本設定")]
        [SerializeField, Tooltip("演出にかける時間")] private float dissolveTime = 2.0f;
        [SerializeField, Tooltip("対象のRenderer")] private Renderer targetRenderer;
        
        [Header("シェーダー設定")]
        [SerializeField, Tooltip("通常時の値（描画されている状態）")] private float startValue = -0.5f; 
        [SerializeField, Tooltip("消滅時の値（完全に消えた状態）")] private float endValue = 0.5f;

        [Header("VFX設定")]
        [SerializeField, Tooltip("死亡時のVFX")] private VisualEffect ashVfx;
        [SerializeField, Tooltip("VFX消去時の待ち時間")] private float vfxWaitTime = 3.5f;
        [SerializeField, Tooltip("パーティクルの最大放出量")] private float maxSpawnRate = 1000.0f;

        private static readonly int DissolveAmountPropId = Shader.PropertyToID("_DissolveAmount");
        private const string SpawnRatePropName = "SpawnRate";
        private Material instanceMaterial;
        private CancellationTokenSource cts;

        private void Start()
        {
            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();

            if (targetRenderer != null)
            {
                instanceMaterial = targetRenderer.material;
                instanceMaterial.SetFloat(DissolveAmountPropId, startValue);
            }

            if (ashVfx != null)
            {
                ashVfx.SetFloat(SpawnRatePropName, 0.0f);
                ashVfx.Stop();
            }

            cts = new CancellationTokenSource();
        }

        /// <summary>
        /// 溶解エフェクトを再生（待機可能版）
        /// </summary>
        public async UniTask PlayDissolveEffectAsync()
        {
            if (instanceMaterial == null) 
                return;
            
            await DissolveAsync(cts.Token);
        }

        private async UniTask DissolveAsync(CancellationToken cancellationToken)
        {
            if (ashVfx != null)
            {
                ashVfx.SetFloat(SpawnRatePropName, maxSpawnRate);
                ashVfx.Play();
            }
            
            float elapsedTime = 0.0f;
            while (elapsedTime < dissolveTime)
            {
                // オブジェクトが破棄されていたら抜ける
                if (cancellationToken.IsCancellationRequested) 
                    return;

                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / dissolveTime;
                float currentAmount = Mathf.Lerp(startValue, endValue, progress);

                if (instanceMaterial != null)
                {
                    instanceMaterial.SetFloat(DissolveAmountPropId, currentAmount);
                }

                if (ashVfx != null)
                {
                    float currentRate = Mathf.Lerp(maxSpawnRate, 0.0f, progress);
                    ashVfx.SetFloat(SpawnRatePropName, currentRate);
                }
                
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
            
            if (instanceMaterial != null)
            {
                instanceMaterial.SetFloat(DissolveAmountPropId, endValue);
            }

            if (ashVfx != null)
            {
                ashVfx.SetFloat(SpawnRatePropName, 0.0f);
                ashVfx.Stop();
            }

            await UniTask.Delay(TimeSpan.FromSeconds(vfxWaitTime), cancellationToken: cancellationToken);
        }

        private void OnDestroy()
        {
            cts?.Cancel();
            cts?.Dispose();
            
            if (instanceMaterial != null)
            {
                Destroy(instanceMaterial);
            }
        }
    }
}
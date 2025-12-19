using System;
using Cysharp.Threading.Tasks;
using RePuzzleKnights.Scripts.InGame.Allies;
using RePuzzleKnights.Scripts.InGame.Allies.SO;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RePuzzleKnights.Scripts.InGame.PlacementSystem
{
    public class PlacementView : MonoBehaviour
    {
        [SerializeField] private Color validColor = Color.green;
        [SerializeField] private Color invalidColor = Color.red;
        
        public GameObject CurrentPreviewObject { get; private set; }
        private Renderer[] previewRenderers;

        public async UniTaskVoid SpawnPreviewAsync(AllyDataSO data, Vector3 initialPosition)
        {
            DestroyPreview();

            var handle = Addressables.InstantiateAsync(data.PrefabRef);
            var obj = await handle.ToUniTask();

            CurrentPreviewObject = obj;
            CurrentPreviewObject.transform.position = initialPosition;

            var colliders = CurrentPreviewObject.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }
            
            previewRenderers = CurrentPreviewObject.GetComponentsInChildren<Renderer>();
            
            var holder = CurrentPreviewObject.GetComponent<AllyEntityHolder>();
            if (holder != null)
            {
                Destroy(holder);
            }
            
            var battlePresenter = CurrentPreviewObject.GetComponent<AllyBattlePresenter>();
            if (battlePresenter != null)
            {
                Destroy(battlePresenter);
            }
        }
        
        /// <summary>
        /// プレビューの向きを更新（2D画像の左右反転）
        /// </summary>
        public void UpdatePreviewRotation(Quaternion rotation)
        {
            if (CurrentPreviewObject == null)
                return;
                
            CurrentPreviewObject.transform.rotation = rotation;
            
            if (CurrentPreviewObject.TryGetComponent<AllyView>(out var allyView))
            {
                allyView.SetInitialDirection(rotation);
            }
        }

        public async UniTaskVoid CreateAllyAsync(AllyDataSO data, Vector3 position, Quaternion rotation, Action<AllyDataSO> onDeath)
        {
            var handle = Addressables.InstantiateAsync(data.PrefabRef);
            var obj = await handle.ToUniTask();

            obj.transform.position = position;
            obj.transform.rotation = rotation;

            // 味方の向きを設定（2D画像の左右反転）
            if (obj.TryGetComponent<AllyView>(out var allyView))
            {
                allyView.SetInitialDirection(rotation);
            }

            if (obj.TryGetComponent<AllyBattlePresenter>(out var presenter))
            {
                presenter.Initialize(onDeath);
            }
        }

        public void UpdatePreviewVisuals(bool isValid)
        {
            if (CurrentPreviewObject == null)
                return;
            
            CurrentPreviewObject.SetActive(isValid);
            
            if (isValid && previewRenderers != null)
            {
                foreach (var r in previewRenderers)
                {
                    if (r != null)
                    {
                        var materials = r.materials;
                        foreach (var mat in materials)
                        {
                            mat.color = validColor;
                        }
                    }
                }
            }
        }

        public void DestroyPreview()
        {
            if (CurrentPreviewObject != null)
            {
                Destroy(CurrentPreviewObject);
                CurrentPreviewObject = null;
            }
            
            previewRenderers = null;
        }
    }
}
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
            
            var battlePresenter = CurrentPreviewObject.GetComponent<AllyBattlePresenter>();
            if (battlePresenter != null)
            {
                battlePresenter.enabled = false;
            }
        }

        public async UniTaskVoid CreateAllyAsync(AllyDataSO data, Vector3 position, Quaternion rotation, Action<AllyDataSO> onDeath)
        {
            var handle = Addressables.InstantiateAsync(data.PrefabRef);
            var obj = await handle.ToUniTask();

            obj.transform.position = position;
            obj.transform.rotation = rotation;

            if (obj.TryGetComponent<AllyBattlePresenter>(out var presenter))
            {
                presenter.Initialize(onDeath);
            }
        }

        public void UpdatePreviewVisuals(bool isValid)
        {
            if (previewRenderers == null || previewRenderers.Length == 0)
                return;

            Color targetColor = isValid ? validColor : invalidColor;
            
            foreach (var r in previewRenderers)
            {
                if (r != null)
                {
                    var materials = r.materials;
                    foreach (var mat in materials)
                    {
                        mat.color = targetColor;
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
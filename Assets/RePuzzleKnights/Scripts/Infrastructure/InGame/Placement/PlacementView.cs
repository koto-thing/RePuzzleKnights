using Cysharp.Threading.Tasks;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies;
using RePuzzleKnights.Scripts.Presentation.InGame;
using UnityEngine;
using UnityEngine.AddressableAssets;

// For AllyView, AllyEntityHolder? No Holder used new.

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.Placement
{
    public class PlacementView : MonoBehaviour, IPlacementView
    {
        [SerializeField] private Color validColor = new Color(0, 1, 0, 0.5f);
        [SerializeField] private Color invalidColor = new Color(1, 0, 0, 0.5f);
        
        private GameObject _currentPreviewObject;
        private Renderer[] _previewRenderers;
        private AssetReferenceGameObject _currentPrefabRef;

        public void SetPreviewPrefab(AssetReferenceGameObject prefabRef)
        {
            _currentPrefabRef = prefabRef;
            // Eager load or wait for ShowPreview?
            // Usually start instantiation here.
            SpawnPreviewAsync(prefabRef).Forget();
        }

        private async UniTaskVoid SpawnPreviewAsync(AssetReferenceGameObject prefabRef)
        {
            DestroyPreview();
            
            if (prefabRef == null || !prefabRef.RuntimeKeyIsValid()) return;

            var handle = Addressables.InstantiateAsync(prefabRef);
            var obj = await handle.ToUniTask();

            _currentPreviewObject = obj;
            // Initially hiding or at zero?
            // Hide untill ShowPreview called.
            _currentPreviewObject.SetActive(false); // Hide via GameObject active or Position off screen?
            // SetActive(false) handles Visibility.
            
            // Setup Visuals (Sensors off, Renderers cached)
            var colliders = _currentPreviewObject.GetComponentsInChildren<Collider>();
            foreach (var col in colliders) col.enabled = false;
            
            // Remove Scripts that might run Logic
            // Clean logic scripts if any.
            // E.g. AllyController logic?
            // Ideally Prefab has logic separately or we disable it.
            // Assuming Prefab is full Ally.
            var allyView = _currentPreviewObject.GetComponent<AllyView>();
            if (allyView != null) Destroy(allyView);

            // Legacy removed AllyEntityHolder and BattlePresenter.
            // I should double check if I have other components to remove.
            
            _previewRenderers = _currentPreviewObject.GetComponentsInChildren<Renderer>();
        }

        public void ShowPreview(Vector3 position, Quaternion rotation, bool isValid)
        {
            if (_currentPreviewObject == null) return;
            
            _currentPreviewObject.SetActive(true);
            _currentPreviewObject.transform.position = position;
            _currentPreviewObject.transform.rotation = rotation;
            
            // Apply Color
            if (_previewRenderers != null)
            {
                Color color = isValid ? validColor : invalidColor;
                foreach (var r in _previewRenderers)
                {
                   if (r.GetType().Name == "VisualEffectRenderer") continue;

                   foreach (var mat in r.materials)
                   {
                       if (mat.HasProperty("_Color")) mat.color = color;
                       else if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                   }
                }
            }
        }

        public void HidePreview()
        {
            if (_currentPreviewObject != null)
            {
                _currentPreviewObject.SetActive(false);
            }
        }

        private void DestroyPreview()
        {
            if (_currentPreviewObject != null)
            {
                Destroy(_currentPreviewObject); // Or ReleaseAddressable?
                // Addressables.ReleaseInstance(currentPreviewObject); // Better practice
                _currentPreviewObject = null;
            }
            _previewRenderers = null;
        }

        private void OnDestroy()
        {
             DestroyPreview();
        }
    }
}


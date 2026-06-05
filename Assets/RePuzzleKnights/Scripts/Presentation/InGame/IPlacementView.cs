using UnityEngine;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    public interface IPlacementView
    {
        void SetPreviewPrefab(UnityEngine.AddressableAssets.AssetReferenceGameObject prefabRef);
        void ShowPreview(Vector3 position, Quaternion rotation, bool isValid);
        void HidePreview();
    }
}



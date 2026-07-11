using UnityEngine;
using System.Collections.Generic;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Domain.Enums;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    public interface IPlacementView
    {
        void SetPreviewPrefab(UnityEngine.AddressableAssets.AssetReferenceGameObject prefabRef);
        void ShowPreview(Vector3 position, Quaternion rotation, bool isValid, List<GridCoordinate> rangeGrids);
        void HidePreview();

        void ShowValidPlacements(PlacementType placementType);
        void HideValidPlacements();

        /// <summary>融合プレビュー：次レベルの攻撃範囲と差分ステータスを表示</summary>
        void ShowFusionPreview(Vector3 targetPosition, Quaternion targetRotation,
            List<GridCoordinate> nextRangeGrids, float hpDiff, float atkDiff, bool isEvolution, string nextJobName);
        void HideFusionPreview();
    }
}



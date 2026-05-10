using RePuzzleKnights.Scripts.Domain.Services;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.Placement
{
    public class UnityPlacementValidator : IPlacementValidator
    {
        private readonly int _allyLayer; 
        
        public UnityPlacementValidator()
        {
            _allyLayer = LayerMask.GetMask("Ally");
        }

        public bool IsPositionOccupied(Vector3 position)
        {
            return GetAllyObjectAtPosition(position) != null;
        }

        public GameObject GetAllyObjectAtPosition(Vector3 position)
        {
            float checkRadius = 0.5f; // 半径を広げて検知しやすくする
            var colliders = Physics.OverlapSphere(position, checkRadius, _allyLayer);
            foreach (var col in colliders)
            {
                // AllyReference（または親にあるもの）を確認
                var reference = col.GetComponentInParent<AllyReference>();
                if (reference != null)
                {
                     float distance = Vector3.Distance(position, reference.transform.position);
                     if (distance < 0.8f) 
                     {
                         Debug.Log($"Ally detected at {position} for fusion: {reference.Ally.Stats.Name}");
                         return reference.gameObject;
                     }
                }
            }
            
            return null;
        }

        public bool IsTerrainValid(Vector3 position, string terrainTag, bool isHighGround)
        {
            if (isHighGround) 
                return terrainTag == "HIGHGROUND_BLOCK";
            
            return terrainTag == "GROUND_BLOCK";
        }
    }
}



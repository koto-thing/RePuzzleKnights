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
            float checkRadius = 0.3f;
            var colliders = Physics.OverlapSphere(position, checkRadius, _allyLayer);
            foreach (var col in colliders)
            {
                if (col.GetComponent<AllyView>() != null)
                {
                     float distance = Vector3.Distance(position, col.transform.position);
                     if (distance < 0.5f) return true;
                }
            }
            
            return false;
        }

        public bool IsTerrainValid(Vector3 position, string terrainTag, bool isHighGround)
        {
            if (isHighGround) 
                return terrainTag == "HIGHGROUND_BLOCK";
            
            return terrainTag == "GROUND_BLOCK";
        }
    }
}



using UnityEngine;

// For AllyStats or similar if needed? 
// Or use AllyDataSO (Infrastructure/Data but referenced in Domain as Data Transfer Object? 
// Ideally Domain shouldn't know SO. 
// But passing AllyStats (Value Object) is fine.
// Legacy used AllyDataSO. I created AllyStats.
// I'll use AllyStats or a subset "PlacementInfo".
// For now, I'll allow Vector3 and simple types.

namespace RePuzzleKnights.Scripts.Domain.Services
{
    public interface IPlacementValidator
    {
        bool IsPositionOccupied(Vector3 position);
        bool IsTerrainValid(Vector3 position, string terrainTag, bool isHighGround); 
        GameObject GetAllyObjectAtPosition(Vector3 position);
    }
}



using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RePuzzleKnights.Scripts.InGame.Enemies.BugEnemy;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    public interface IEnemyFactoryService
    {
        UniTask<GameObject> CreateEnemyAsync(AssetReferenceGameObject prefabRef, BugEnemyDataSO data, Vector3 spawnPosition);
        List<Vector3> CalculatePath(Vector3 spawnPosition, BugEnemyDataSO data);
    }
}
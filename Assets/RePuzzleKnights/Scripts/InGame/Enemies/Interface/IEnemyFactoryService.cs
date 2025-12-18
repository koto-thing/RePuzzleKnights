using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RePuzzleKnights.Scripts.InGame.Enemies;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    /// <summary>
    /// 敵生成サービスのインターフェース
    /// 敵の生成と経路計算を担当
    /// </summary>
    public interface IEnemyFactoryService
    {
        /// <summary>
        /// 敵を非同期で生成
        /// </summary>
        UniTask<GameObject> CreateEnemyAsync(AssetReferenceGameObject prefabRef, EnemyDataSO data, Vector3 spawnPosition);
        
        /// <summary>
        /// 敵の移動経路を計算
        /// </summary>
        List<Vector3> CalculatePath(Vector3 spawnPosition, EnemyDataSO data);
    }
}
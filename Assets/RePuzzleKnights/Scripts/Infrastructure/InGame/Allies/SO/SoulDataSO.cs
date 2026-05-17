using RePuzzleKnights.Scripts.Domain.Enums;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.Enum;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO
{
    /// <summary>
    /// 魂（進化前ユニット）のデータ定義。
    /// AllyDataSOを継承することで、魂の状態でも詳細なステータス設定を可能にする。
    /// </summary>
    [CreateAssetMenu(fileName = "New SoulData", menuName = "Allies/Create New SoulData")]
    public class SoulDataSO : AllyDataSO
    {
        [Header("魂専用設定")]
        public Sprite Icon;

        // AllyName, Element, AllyType, PrefabRef などの基本ステータスは AllyDataSO から継承
    }
}

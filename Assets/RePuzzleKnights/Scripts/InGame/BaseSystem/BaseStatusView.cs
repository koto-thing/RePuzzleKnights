using TMPro;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.BaseSystem
{
    /// <summary>
    /// 本拠地ステータスの表示を管理するViewクラス
    /// 耐久値の表示を担当
    /// </summary>
    public class BaseStatusView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI durabilityText;

        /// <summary>
        /// 耐久度UIを更新する
        /// </summary>
        /// <param name="durability">現在の耐久度</param>
        /// <param name="maxDurability">最大耐久度</param>
        public void UpdateDurabilityDisplay(int durability, int maxDurability)
        {
            durabilityText.text = $"{durability} / {maxDurability}";
        }
    }
}
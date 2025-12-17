using TMPro;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.BaseSystem
{
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
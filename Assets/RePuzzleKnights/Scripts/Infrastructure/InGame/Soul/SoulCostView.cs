using System.Collections.Generic;
using UnityEngine;
using RePuzzleKnights.Scripts.Presentation.InGame;
using RePuzzleKnights.Scripts.Domain.Enums;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.Soul
{
    /// <summary>
    /// 各属性ごとの所持SoulコストをOnGUIでデバッグ表示するView。
    /// </summary>
    public class SoulCostView : MonoBehaviour, ISoulCostView
    {
        [Header("Debug UI Settings")]
        [SerializeField] private bool showDebugUI = true;
        [SerializeField] private int fontSize = 18;
        [SerializeField] private Color textColor = Color.yellow;

        private readonly Dictionary<ElementType, int> _soulCosts = new();

        public void SetSoulCost(ElementType element, int cost)
        {
            _soulCosts[element] = cost;
        }

        private void OnGUI()
        {
            if (!showDebugUI) return;

            GUIStyle style = new GUIStyle();
            style.fontSize = fontSize;
            style.normal.textColor = textColor;
            style.fontStyle = FontStyle.Bold;

            // 背景に薄い黒を敷く
            GUI.Box(new Rect(10, 10, 240, 130), "");

            int yOffset = 15;
            foreach (var kvp in _soulCosts)
            {
                GUI.Label(new Rect(20, yOffset, 220, 25), $"{kvp.Key} Soul: {kvp.Value}", style);
                yOffset += 20;
            }
        }
    }
}

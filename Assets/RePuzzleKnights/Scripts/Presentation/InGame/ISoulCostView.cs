using RePuzzleKnights.Scripts.Domain.Enums;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    /// <summary>
    /// 各属性ごとのSoulコスト表示UIを抽象化するインターフェース。
    /// </summary>
    public interface ISoulCostView
    {
        /// <summary>
        /// 指定した属性の所持SoulコストをUIに設定する
        /// </summary>
        void SetSoulCost(ElementType element, int cost);
    }
}

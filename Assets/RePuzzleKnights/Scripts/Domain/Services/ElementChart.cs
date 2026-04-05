using RePuzzleKnights.Scripts.Domain.Enums;

namespace RePuzzleKnights.Scripts.Domain.Services
{
    /// <summary>
    /// 属性相性の倍率を計算する
    /// 3すくみ: 火 > 草 > 水 > 火
    /// 2すくみ: 光 > 闇 > 光
    /// ノーマル: 相性なし
    /// </summary>
    public static class ElementChart
    {
        private const float Advantage = 1.5f;
        private const float Disadvantage = 0.8f;
        private const float Neutral   = 1.0f;

        public static float GetMultiplier(ElementType attacker, ElementType defender)
        {
            return (attacker, defender) switch
            {
                (ElementType.Fire,  ElementType.Grass) => Advantage,
                (ElementType.Grass, ElementType.Water) => Advantage,
                (ElementType.Water, ElementType.Fire)  => Advantage,
                (ElementType.Light, ElementType.Dark)  => Advantage,
                (ElementType.Dark,  ElementType.Light) => Advantage,
                (ElementType.Grass, ElementType.Fire)  => Disadvantage,
                (ElementType.Water, ElementType.Grass) => Disadvantage,
                (ElementType.Fire,  ElementType.Water) => Disadvantage,
                _                                      => Neutral,
            };
        }
    }
}
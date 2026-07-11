namespace RePuzzleKnights.Scripts.Domain.Entities
{
    /// <summary>
    /// ドメイン層でグリッド座標を表す値オブジェクト（UnityのVector2Intへの依存を避けるためのPOCO設計）
    /// </summary>
    public struct GridCoordinate
    {
        public int X { get; }
        public int Y { get; }

        public GridCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}

namespace RePuzzleKnights.Scripts.Domain.Entities
{
    public struct Edge
    {
        public string To { get; }
        public int Weight { get; }

        public Edge(string to, int weight)
        {
            To = to;
            Weight = weight;
        }
    }
}



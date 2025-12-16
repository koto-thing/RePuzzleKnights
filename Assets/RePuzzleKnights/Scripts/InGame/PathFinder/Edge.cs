namespace RePuzzleKnights.Scripts.InGame.PathFinder
{
    public class Edge
    {
        private string to;
        private int weight;

        public string To => to;
        public int Weight => weight;

        public Edge(string to, int weight)
        {
            this.to = to;
            this.weight = weight;
        }
    }
}
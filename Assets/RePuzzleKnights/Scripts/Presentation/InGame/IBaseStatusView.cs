namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    public interface IBaseStatusView
    {
        void UpdateDurability(int current, int max);
        void UpdateEnemyCount(int defeated, int total);
        void PlayDestroyEffect();
    }
}



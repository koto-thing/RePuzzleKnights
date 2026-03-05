namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    public interface IBaseStatusView
    {
        void UpdateDurability(int current, int max);
        void PlayDestroyEffect();
    }
}



using R3;

namespace RePuzzleKnights.Scripts.Domain.Services
{
    public interface ILevelStatusProvider
    {
        ReadOnlyReactiveProperty<bool> IsAllWavesFinished { get; }
        ReadOnlyReactiveProperty<int> ActiveEnemyCount { get; }
        ReadOnlyReactiveProperty<int> DefeatedEnemyCount { get; }
        int TotalEnemyCount { get; }
    }
}



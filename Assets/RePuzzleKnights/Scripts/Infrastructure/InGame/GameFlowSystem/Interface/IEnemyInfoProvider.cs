using R3;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.GameFlowSystem.Interface
{
    public interface IEnemyInfoProvider
    {
        ReadOnlyReactiveProperty<int> ActiveEnemyCount { get; }
    }
}



using R3;

namespace RePuzzleKnights.Scripts.InGame.GameFlowSystem.Interface
{
    public interface IEnemyInfoProvider
    {
        ReadOnlyReactiveProperty<int> ActiveEnemyCount { get; }
    }
}
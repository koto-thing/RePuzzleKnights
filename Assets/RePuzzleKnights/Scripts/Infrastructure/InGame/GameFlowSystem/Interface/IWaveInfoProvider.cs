using R3;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.GameFlowSystem.Interface
{
    public interface IWaveInfoProvider
    {
        ReadOnlyReactiveProperty<bool> IsAllWavesFinished { get; }
    }
}



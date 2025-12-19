using R3;

namespace RePuzzleKnights.Scripts.InGame.GameFlowSystem.Interface
{
    public interface IWaveInfoProvider
    {
        ReadOnlyReactiveProperty<bool> IsAllWavesFinished { get; }
    }
}
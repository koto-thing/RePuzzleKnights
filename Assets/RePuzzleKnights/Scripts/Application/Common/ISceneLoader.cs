using Cysharp.Threading.Tasks;

namespace RePuzzleKnights.Scripts.Application.Common
{
    public interface ISceneLoader
    {
        UniTask LoadSceneAsync(string key);
    }
}
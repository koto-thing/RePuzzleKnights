using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RePuzzleKnights.Scripts.Domain.Entities;

namespace RePuzzleKnights.Scripts.Domain.Repositories
{
    public interface IStageRepository
    {
        void SaveProgress(int nextStageNumber);
        int GetCurrentStageNumber();
        UniTask<IEnumerable<Stage>> GetAllStagesAsync();
        UniTask<UnityEngine.Sprite> GetStageImageAsync(int stageNumber);
        UniTask<UnityEngine.Sprite> GetCharaImageAsync(int stageNumber);
        UniTask<UnityEngine.AddressableAssets.AssetReference> GetStageSceneRefAsync(int stageNumber);
    }
}



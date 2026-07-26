using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Domain.Repositories;
using RePuzzleKnights.Scripts.Infrastructure.Common;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame
{
    public class StageDataRepository : IStageRepository
    {
        private readonly StageProgressService _progressService;
        private readonly CurrentStageService _currentStageService;

        public StageDataRepository(StageProgressService progressService, CurrentStageService currentStageService)
        {
            this._progressService = progressService;
            this._currentStageService = currentStageService;
        }

        public void SaveProgress(int nextStageNumber)
        {
            _progressService.SaveProgress(nextStageNumber);
        }

        public int GetCurrentStageNumber()
        {
            return _currentStageService.GetCurrentStageNumber();
        }
        
        public async UniTask<IEnumerable<Stage>> GetAllStagesAsync()
        {
            // TODO: Load stage data from Addressables or ScriptableObjects
            await UniTask.CompletedTask;
            return new List<Stage>();
        }
        
        public async UniTask<Sprite> GetStageImageAsync(int stageNumber)
        {
            // TODO: Load stage image from Addressables
            await UniTask.CompletedTask;
            return null;
        }
        
        public async UniTask<Sprite> GetCharaImageAsync(int stageNumber)
        {
            // TODO: Load character image from Addressables
            await UniTask.CompletedTask;
            return null;
        }
        
        public async UniTask<AssetReference> GetStageSceneRefAsync(int stageNumber)
        {
            // TODO: Load scene reference from stage data
            await UniTask.CompletedTask;
            return null;
        }
    }
}




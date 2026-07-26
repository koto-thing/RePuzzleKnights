using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Domain.Repositories;
using RePuzzleKnights.Scripts.Infrastructure.Common;
using RePuzzleKnights.Scripts.Infrastructure.StageSelect.Definitions;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RePuzzleKnights.Scripts.Infrastructure.StageSelect
{
    public class StageRepository : IStageRepository
    {
        private readonly StageProgressService _progressService;
        private readonly StageSelectDataSO _config;
        
        private List<Stage> _cachedStages;

        public StageRepository(StageProgressService progressService, StageSelectDataSO config)
        {
            this._progressService = progressService;
            this._config = config;
        }

        public void SaveProgress(int nextStageNumber)
        {
            _progressService.SaveProgress(nextStageNumber);
            _cachedStages = null;
        }

        public int GetCurrentStageNumber()
        {
            return _progressService.GetCurrentProgress();
        }

        public async UniTask<IEnumerable<Stage>> GetAllStagesAsync()
        {
            if (_cachedStages != null) 
                return _cachedStages;

            int maxCleared = GetCurrentStageNumber();
            var list = new List<Stage>();

            if (_config != null && _config.StageDescriptionDataRefs != null)
            {
                var tasks = new List<UniTask<StageDescriptionDataSO>>();
                foreach (var refData in _config.StageDescriptionDataRefs)
                {
                    if (refData != null && refData.RuntimeKeyIsValid())
                    {
                        tasks.Add(LoadStageData(refData));
                    }
                }

                var results = await UniTask.WhenAll(tasks);
                
                foreach (var data in results)
                {
                    if (data == null) 
                        continue;
                    
                    bool isUnlocked = _progressService.IsStageUnlocked(data.StageNumber);
                    bool isCleared = data.StageNumber < maxCleared; 
                    
                    var stage = new Stage(data.StageNumber, data.StageName, data.StageDescription, isUnlocked, isCleared);
                    list.Add(stage);
                }
            }
            
            _cachedStages = list.OrderBy(s => s.Id).ToList();
            
            return _cachedStages;
        }

        private async UniTask<StageDescriptionDataSO> LoadStageData(AssetReference refData)
        {
            var handle = Addressables.LoadAssetAsync<StageDescriptionDataSO>(refData);
            return await handle.ToUniTask();
        }

        public async UniTask<Sprite> GetStageImageAsync(int stageNumber)
        {
            if (_config == null || _config.StageDescriptionDataRefs == null) 
                return null;
             
            foreach(var refData in _config.StageDescriptionDataRefs)
            {
                if (refData != null && refData.RuntimeKeyIsValid())
                {
                    var data = await LoadStageData(refData);
                    if (data != null && data.StageNumber == stageNumber)
                    {
                        return data.Image;
                    }
                }
            }
            
            return null;
        }
        
        public async UniTask<Sprite> GetCharaImageAsync(int stageNumber)
        {
            if (_config == null || _config.StageDescriptionDataRefs == null) 
                return null;
             
            foreach(var refData in _config.StageDescriptionDataRefs)
            {
                if (refData != null && refData.RuntimeKeyIsValid())
                {
                    var data = await LoadStageData(refData);
                    if (data != null && data.StageNumber == stageNumber)
                    {
                        return data.Chara;
                    }
                }
            }
            
            return null;
        }

        public async UniTask<AssetReference> GetStageSceneRefAsync(int stageNumber)
        {
            foreach(var refData in _config.StageDescriptionDataRefs)
            {
                if (refData != null && refData.RuntimeKeyIsValid())
                {
                    var data = await LoadStageData(refData);
                    if (data != null && data.StageNumber == stageNumber)
                    {
                        return data.SceneRef;
                    }
                }
            }
            
            return null;
        }
    }
}



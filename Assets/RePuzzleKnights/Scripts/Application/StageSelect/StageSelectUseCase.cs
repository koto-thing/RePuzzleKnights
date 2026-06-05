using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Domain.Repositories;

namespace RePuzzleKnights.Scripts.Application.StageSelect
{
    public class StageSelectUseCase
    {
        private readonly IStageRepository _stageRepository;
        
        public ReadOnlyReactiveProperty<IEnumerable<Stage>> Stages => _stages;
        private readonly ReactiveProperty<IEnumerable<Stage>> _stages = new();

        public ReadOnlyReactiveProperty<Stage> SelectedStage => _selectedStage;
        private readonly ReactiveProperty<Stage> _selectedStage = new();
        
        public Observable<Stage> OnStageStarted => _onStageStarted;
        private readonly Subject<Stage> _onStageStarted = new();

        public StageSelectUseCase(IStageRepository stageRepository)
        {
            _stageRepository = stageRepository;
        }

        /// <summary>
        /// ステージをロード
        /// </summary>
        public async UniTask LoadStagesAsync()
        {
            var stages = await _stageRepository.GetAllStagesAsync();
            _stages.Value = stages;
            
            var latest = stages.Where(s => s.IsUnlocked).OrderByDescending(s => s.Id).FirstOrDefault();
            if (latest != null)
            {
                _selectedStage.Value = latest;
            }
        }

        /// <summary>
        /// ステージ選択
        /// </summary>
        /// <param name="stageId">ステージのID</param>
        public void SelectStage(int stageId)
        {
            var stages = _stages.Value;
            if (stages == null)
            {
                UnityEngine.Debug.LogWarning("[StageSelectUseCase] Stages list is null");
                return;
            }
            
            var stage = stages.FirstOrDefault(s => s.Id == stageId);
            if (stage == null)
            {
                UnityEngine.Debug.LogWarning($"[StageSelectUseCase] Stage with Id={stageId} not found");
                return;
            }
            
            if (stage.IsUnlocked)
            {
                _selectedStage.Value = stage;
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[StageSelectUseCase] Stage {stageId} is locked");
            }
        }

        /// <summary>
        /// 選択したステージを開始
        /// </summary>
        public void StartSelectedStage()
        {
            var stage = _selectedStage.Value;
            if (stage is { IsUnlocked: true })
            {
                _onStageStarted.OnNext(stage);
            }
        }
    }
}



using R3;
using RePuzzleKnights.Scripts.Domain.Enums;
using RePuzzleKnights.Scripts.Domain.Repositories;

namespace RePuzzleKnights.Scripts.Application.InGame
{
    public class GameFlowUseCase
    {
        private readonly IStageRepository _stageRepository;

        public ReadOnlyReactiveProperty<GameResultState> CurrentState => _currentState;
        private readonly ReactiveProperty<GameResultState> _currentState = new(GameResultState.PLAYING);

        public GameFlowUseCase(IStageRepository stageRepository)
        {
            _stageRepository = stageRepository;
        }

        /// <summary>
        /// ゲームの状態を遷移させる
        /// </summary>
        /// <param name="newState">遷移先</param>
        public void TransitionState(GameResultState newState)
        {
            if (_currentState.Value != GameResultState.PLAYING)
                return;

            _currentState.Value = newState;

            if (newState == GameResultState.GAME_CLEAR)
            {
                SaveProgress();
            }
        }

        /// <summary>
        /// クリアしたステージの進捗を保存する
        /// </summary>
        private void SaveProgress()
        {
            int currentStage = _stageRepository.GetCurrentStageNumber();
            if (currentStage >= 0)
            {
                _stageRepository.SaveProgress(currentStage + 1);
            }
        }
    }
}



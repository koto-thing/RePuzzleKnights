using R3;
using RePuzzleKnights.Scripts.Common;
using RePuzzleKnights.Scripts.InGame.GameFlowSystem.Enum;

namespace RePuzzleKnights.Scripts.InGame.GameFlowSystem
{
    /// <summary>
    /// ゲームフローの状態を管理するModelクラス
    /// ゲームの進行状態（プレイ中、クリア、ゲームオーバー）を管理
    /// </summary>
    public class GameFlowModel
    {
        private readonly StageProgressService progressService;
        private readonly CurrentStageService currentStageService;
        
        public ReadOnlyReactiveProperty<GameResultState> CurrentState => currentState;
        private readonly ReactiveProperty<GameResultState> currentState = new(GameResultState.PLAYING);

        public GameFlowModel(StageProgressService progressService, CurrentStageService currentStageService)
        {
            // VContainerから注入されたインスタンスを使用するが、シングルトンインスタンスが優先される
            this.progressService = StageProgressService.Instance;
            this.currentStageService = CurrentStageService.Instance;
        }
        
        /// <summary>
        /// ゲーム状態を遷移させる
        /// プレイ中の状態からのみ遷移可能
        /// </summary>
        public void TransitionState(GameResultState newState)
        {
            if (currentState.Value != GameResultState.PLAYING) 
                return;
            
            currentState.Value = newState;
            UnityEngine.Debug.Log($"[GameFlowModel] State transitioned to: {newState}");
            
            // ゲームクリア時に進捗を保存
            if (newState == GameResultState.GAME_CLEAR)
            {
                SaveProgress();
            }
        }
        
        /// <summary>
        /// 現在のステージクリア情報を保存
        /// </summary>
        private void SaveProgress()
        {
            int currentStage = currentStageService.GetCurrentStageNumber();
            
            UnityEngine.Debug.Log($"[GameFlowModel] SaveProgress called. Current stage from service: {currentStage}");
            
            if (currentStage >= 0)
            {
                // 次のステージを開放
                progressService.SaveProgress(currentStage + 1);
                UnityEngine.Debug.Log($"[GameFlowModel] Stage {currentStage} cleared! Next stage {currentStage + 1} unlocked.");
            }
            else
            {
                UnityEngine.Debug.LogWarning("[GameFlowModel] Current stage number is invalid! Stage number was not set before starting the game.");
            }
        }
    }
}
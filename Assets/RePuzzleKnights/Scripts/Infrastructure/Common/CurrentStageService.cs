namespace RePuzzleKnights.Scripts.Infrastructure.Common
{
    /// <summary>
    /// 現在プレイ中のステージ情報を保持するサービスクラス
    /// シーン間でステージ番号を共有するために使用
    /// </summary>
    public class CurrentStageService
    {
        private int currentStageNumber = -1;
        
        /// <summary>
        /// 現在のステージ番号を取得
        /// </summary>
        public int GetCurrentStageNumber()
        {
            return currentStageNumber;
        }
        
        /// <summary>
        /// 現在のステージ番号を設定
        /// </summary>
        /// <param name="stageNumber">ステージ番号</param>
        public void SetCurrentStageNumber(int stageNumber)
        {
            currentStageNumber = stageNumber;
            UnityEngine.Debug.Log($"[CurrentStageService] Current stage set to: {stageNumber}");
        }
        
        /// <summary>
        /// リセット
        /// </summary>
        public void Reset()
        {
            currentStageNumber = -1;
        }
    }
}



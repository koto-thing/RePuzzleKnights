namespace RePuzzleKnights.Scripts.Common
{
    /// <summary>
    /// 現在プレイ中のステージ情報を保持するサービスクラス
    /// シーン間でステージ番号を共有するために使用
    /// シングルトンパターンで実装
    /// </summary>
    public class CurrentStageService
    {
        private static CurrentStageService instance;
        public static CurrentStageService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CurrentStageService();
                }
                return instance;
            }
        }
        
        private int currentStageNumber = -1;
        
        // VContainer用のコンストラクタ（シングルトンインスタンスを返す）
        public CurrentStageService()
        {
            if (instance == null)
            {
                instance = this;
            }
        }
        
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


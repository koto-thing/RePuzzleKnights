using UnityEngine;

namespace RePuzzleKnights.Scripts.Common
{
    /// <summary>
    /// ステージの進捗状況を管理するサービスクラス
    /// PlayerPrefsを使用してデータを永続化
    /// シングルトンパターンで実装
    /// </summary>
    public class StageProgressService
    {
        private static StageProgressService instance;
        public static StageProgressService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new StageProgressService();
                }
                return instance;
            }
        }
        
        private const string PROGRESS_KEY = "StageProgress";
        private const int MAX_STAGE_COUNT = 6; // Stage 0-5
        
        // VContainer用のコンストラクタ（シングルトンインスタンスを返す）
        public StageProgressService()
        {
            if (instance == null)
            {
                instance = this;
            }
        }
        
        /// <summary>
        /// 現在の進捗を取得
        /// </summary>
        /// <returns>クリアした最大ステージ番号（0が最小）</returns>
        public int GetCurrentProgress()
        {
            return PlayerPrefs.GetInt(PROGRESS_KEY, 0);
        }
        
        /// <summary>
        /// 進捗を保存
        /// </summary>
        /// <param name="stageNumber">クリアしたステージ番号</param>
        public void SaveProgress(int stageNumber)
        {
            int currentProgress = GetCurrentProgress();
            
            // 現在の進捗より大きい場合のみ更新
            if (stageNumber > currentProgress)
            {
                int newProgress = Mathf.Min(stageNumber, MAX_STAGE_COUNT - 1);
                PlayerPrefs.SetInt(PROGRESS_KEY, newProgress);
                PlayerPrefs.Save();
                
                Debug.Log($"[StageProgressService] Progress saved: Stage {newProgress} unlocked");
            }
        }
        
        /// <summary>
        /// 指定されたステージがプレイ可能かどうかを判定
        /// </summary>
        /// <param name="stageNumber">確認するステージ番号</param>
        /// <returns>プレイ可能ならtrue</returns>
        public bool IsStageUnlocked(int stageNumber)
        {
            return stageNumber <= GetCurrentProgress();
        }
        
        /// <summary>
        /// 進捗をリセット（デバッグ用）
        /// </summary>
        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey(PROGRESS_KEY);
            PlayerPrefs.Save();
            Debug.Log("[StageProgressService] Progress reset");
        }
    }
}


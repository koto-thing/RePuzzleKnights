using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.Common
{
    /// <summary>
    /// ステージの進捗状況を管理するサービスクラス
    /// PlayerPrefsを使用してデータを永続化
    /// </summary>
    public class StageProgressService
    {
        private const string PROGRESS_KEY = "StageProgress";
        private const int MAX_STAGE_COUNT = 6; // Stage 1-6
        
        /// <summary>
        /// 現在の進捗を取得
        /// </summary>
        /// <returns>次にプレイできる最大ステージ番号（初期値: 1）</returns>
        public int GetCurrentProgress()
        {
            return PlayerPrefs.GetInt(PROGRESS_KEY, 1);
        }
        
        /// <summary>
        /// 進捗を保存
        /// ステージをクリアすると、次のステージがアンロックされる
        /// </summary>
        /// <param name="clearedStageNumber">クリアしたステージ番号</param>
        public void SaveProgress(int clearedStageNumber)
        {
            int currentProgress = GetCurrentProgress();
            
            // クリアしたステージの次のステージをアンロック
            int nextStage = clearedStageNumber + 1;
            
            // 現在の進捗より大きい場合のみ更新
            if (nextStage > currentProgress)
            {
                int newProgress = Mathf.Min(nextStage, MAX_STAGE_COUNT);
                PlayerPrefs.SetInt(PROGRESS_KEY, newProgress);
                PlayerPrefs.Save();
            }
        }
        
        /// <summary>
        /// 指定されたステージがプレイ可能かどうかを判定
        /// </summary>
        /// <param name="stageNumber">確認するステージ番号</param>
        /// <returns>プレイ可能ならtrue</returns>
        public bool IsStageUnlocked(int stageNumber)
        {
            int currentProgress = GetCurrentProgress();
            return stageNumber <= currentProgress;
        }
        
        /// <summary>
        /// 進捗をリセット
        /// </summary>
        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey(PROGRESS_KEY);
            PlayerPrefs.Save();
            Debug.Log("[StageProgressService] Progress reset");
        }
    }
}



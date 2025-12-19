using Cysharp.Threading.Tasks;
using R3;
using RePuzzleKnights.Scripts.Common;
using UnityEngine.AddressableAssets;

namespace RePuzzleKnights.Scripts.StageSelect.StageSelect
{
    /// <summary>
    /// ステージ選択のデータと状態を管理するModelクラス
    /// </summary>
    public class StageSelectModel
    {
        private readonly StageProgressService progressService;
        private readonly CurrentStageService currentStageService;
        
        private readonly ReactiveProperty<StageSelectDataSO> stageSelectData = new();
        public Observable<StageSelectDataSO> OnLoadStageSelectData => stageSelectData.AsObservable();

        private readonly ReactiveProperty<StageDescriptionDataSO> stageDescriptionData = new();
        public Observable<StageDescriptionDataSO> OnChangeStageDescriptionData => stageDescriptionData.AsObservable();

        public int CurrentProgress => progressService.GetCurrentProgress();

        public StageSelectModel(StageProgressService progressService, CurrentStageService currentStageService)
        {
            // VContainerから注入されたインスタンスを使用するが、シングルトンインスタンスが優先される
            this.progressService = StageProgressService.Instance;
            this.currentStageService = CurrentStageService.Instance;
        }

        /// <summary>
        /// ステージ選択画面のデータをロードする
        /// </summary>
        public async UniTask LoadStageSelectDataAsync()
        {
            var loadedData = await Addressables.LoadAssetAsync<StageSelectDataSO>("StageSelectData").ToUniTask();
            var instance = UnityEngine.Object.Instantiate(loadedData);
            
            // 進捗を反映
            instance.CurrentProgress = progressService.GetCurrentProgress();
            
            stageSelectData.Value = instance;
        }

        /// <summary>
        /// ステージの詳細情報データをロードする
        /// </summary>
        /// <param name="stageNum">ロードするステージ</param>
        public async UniTask LoadStageDataAsync(int stageNum)
        {
            // ステージがアンロックされているかチェック
            if (!progressService.IsStageUnlocked(stageNum))
            {
                UnityEngine.Debug.LogWarning($"[StageSelectModel] Stage {stageNum} is locked!");
                return;
            }
            
            AssetReference stageDescDataRef = stageSelectData.Value.StageDescriptionDataRefs[stageNum];
            var loadedData = await Addressables.LoadAssetAsync<StageDescriptionDataSO>(stageDescDataRef).ToUniTask();
            var instance = UnityEngine.Object.Instantiate(loadedData);
            
            stageDescriptionData.Value = instance;
            
            // 現在選択中のステージを保存
            UnityEngine.Debug.Log($"[StageSelectModel] Setting current stage to {instance.StageNumber} (from descriptor)");
            currentStageService.SetCurrentStageNumber(instance.StageNumber);
        }

        /// <summary>
        /// 選択されたステージのシーンをロードする
        /// </summary>
        public async UniTask LoadStageSceneAsync()
        {
            if (stageDescriptionData.Value != null)
            {
                await Addressables.LoadSceneAsync(stageDescriptionData.Value.SceneRef).ToUniTask();
            }
        }
    }
}
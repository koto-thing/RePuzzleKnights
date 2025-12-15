using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.AddressableAssets;

namespace RePuzzleKnights.Scripts.StageSelect.StageSelect
{
    public class StageSelectModel
    {
        private readonly ReactiveProperty<StageSelectDataSO> stageSelectData = new();
        public Observable<StageSelectDataSO> OnLoadStageSelectData => stageSelectData.AsObservable();

        private readonly ReactiveProperty<StageDescriptionDataSO> stageDescriptionData = new();
        public Observable<StageDescriptionDataSO> OnChangeStageDescriptionData => stageDescriptionData.AsObservable();

        public int CurrentProgress => stageSelectData.Value?.CurrentProgress ?? 0;

        /// <summary>
        /// ステージ選択画面のデータをロードする
        /// </summary>
        public async UniTask LoadStageSelectDataAsync()
        {
            var loadedData = await Addressables.LoadAssetAsync<StageSelectDataSO>("StageSelectData").ToUniTask();
            stageSelectData.Value = UnityEngine.Object.Instantiate(loadedData);
        }

        /// <summary>
        /// ステージの詳細情報データをロードする
        /// </summary>
        /// <param name="stageNum">ロードするステージ</param>
        public async UniTask LoadStageDataAsync(int stageNum)
        {
            AssetReference stageDescDataRef = stageSelectData.Value.StageDescriptionDataRefs[stageNum];
            var loadedData = await Addressables.LoadAssetAsync<StageDescriptionDataSO>(stageDescDataRef).ToUniTask();
            stageDescriptionData.Value = UnityEngine.Object.Instantiate(loadedData);
        }

        /// <summary>
        /// 選択されたステージのシーンをロードする
        /// </summary>
        public async UniTask LoadStageSceneAsync()
        {
            await Addressables.LoadSceneAsync(stageDescriptionData.Value.SceneRef).ToUniTask();
        }
    }
}
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace RePuzzleKnights.Scripts.Title.ButtonControl
{
    public class ButtonModel
    {
        /// <summary>
        /// スタートボタンをクリックしたときの処理
        /// </summary>
        public async UniTaskVoid OnStartButtonClicked()
        {
            await Addressables.LoadSceneAsync("StageSelectScene").ToUniTask();
        }

        /// <summary>
        /// 設定ボタンをクリックしたときの処理
        /// </summary>
        public void OnSettingsButtonClicked()
        {
            
        }

        /// <summary>
        /// 終了ボタンをクリックしたときの処理
        /// </summary>
        public void OnQuitButtonClicked()
        {
            
        }
    }
}
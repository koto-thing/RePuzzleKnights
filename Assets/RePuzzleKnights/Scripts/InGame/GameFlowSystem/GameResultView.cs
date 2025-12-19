using Cysharp.Threading.Tasks;
using RePuzzleKnights.Scripts.InGame.GameFlowSystem.Enum;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace RePuzzleKnights.Scripts.InGame.GameFlowSystem
{
    /// <summary>
    /// ゲーム結果の表示を管理するViewクラス
    /// クリア・ゲームオーバー画面の表示とボタン操作を担当
    /// </summary>
    public class GameResultView : MonoBehaviour
    {
        [Header("UIオブジェクト")] 
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button backButton;

        private void Awake()
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }
            
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackButtonClicked);
            }
        }

        /// <summary>
        /// 結果画面を表示
        /// </summary>
        public void ShowResult(GameResultState state)
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
            }

            if (resultText != null)
            {
                if (state == GameResultState.GAME_CLEAR)
                {
                    resultText.text = "Game Clear!";
                    resultText.color = Color.green;
                }
                else if (state == GameResultState.GAME_OVER)
                {
                    resultText.text = "Game Over...";
                    resultText.color = Color.red;
                }
            }
        }

        /// <summary>
        /// 戻るボタンが押されたときの処理
        /// </summary>
        private void OnBackButtonClicked()
        {
            LoadStageSelectSceneAsync().Forget();
        }

        /// <summary>
        /// ステージ選択シーンをロード
        /// </summary>
        private async UniTaskVoid LoadStageSelectSceneAsync()
        {
            try
            {
                await Addressables.LoadSceneAsync("StageSelectScene").ToUniTask();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameResultView] Failed to load StageSelectScene: {ex.Message}");
            }
        }

        private void OnDestroy()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBackButtonClicked);
            }
        }
    }
}
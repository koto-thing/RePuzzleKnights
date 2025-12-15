using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RePuzzleKnights.Scripts.StageSelect.StageSelect
{
    public class StageSelectView : MonoBehaviour
    {
        [Header("ステージの詳細表示画面")]
        [SerializeField] private GameObject stageDescriptionPanel;
        [SerializeField] private Button startButton;
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI stageTitleText;
        [SerializeField] private TextMeshProUGUI stageDescriptionText;
        [SerializeField] private Image stageImage;

        public Observable<Unit> OnStartButtonClicked =>
            startButton.OnClickAsObservable();

        public Observable<Unit> OnBackButtonClicked =>
            backButton.OnClickAsObservable();

        /// <summary>
        /// ステージの情報を詳細表示画面にセット
        /// </summary>
        /// <param name="stageDescriptionData"></param>
        public void SetStageData(StageDescriptionDataSO stageDescriptionData)
        {
            if (stageDescriptionData == null)
                return;

            stageTitleText.text = stageDescriptionData.StageName;
            stageDescriptionText.text = stageDescriptionData.StageDescription;
            stageImage.sprite = stageDescriptionData.Image;
        }

        /// <summary>
        /// ステージの詳細表示画面を表示
        /// </summary>
        public void ShowStageSelectPanel()
        {
            startButton.interactable = false;
            backButton.interactable = false;

            stageDescriptionPanel.SetActive(true);
            stageDescriptionPanel.transform.DOLocalMoveX(0, 0.5f)
                .OnComplete(() =>
                {
                    startButton.interactable = true;
                    backButton.interactable = true;
                });
        }

        /// <summary>
        /// ステージの詳細表示画面を非表示
        /// </summary>
        public void HideStageSelectPanel()
        {
            startButton.interactable = false;
            backButton.interactable = false;

            stageDescriptionPanel.transform.DOLocalMoveX(800, 0.5f)
                .OnComplete(() =>
                {
                    stageDescriptionPanel.SetActive(false);
                    startButton.interactable = true;
                    backButton.interactable = true;
                });
        }
    }
}
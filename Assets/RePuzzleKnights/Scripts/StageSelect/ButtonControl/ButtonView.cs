using R3;
using UnityEngine;
using UnityEngine.UI;

namespace RePuzzleKnights.Scripts.StageSelect.ButtonControl
{
    public class ButtonView : MonoBehaviour
    {
        [Header("HUDボタンUI")]
        [SerializeField] private Button backButton;
        
        [Header("ステージセレクトボタンUI")] 
        [SerializeField] private Button stage0Button;
        [SerializeField] private Button stage1Button;
        [SerializeField] private Button stage2Button;
        [SerializeField] private Button stage3Button;
        [SerializeField] private Button stage4Button;
        [SerializeField] private Button stage5Button;
        
        public Observable<Unit> OnBackButtonClicked => 
            backButton.OnClickAsObservable();
        
        public Observable<Unit> OnStage0ButtonClicked => 
            stage0Button.OnClickAsObservable();
        
        public Observable<Unit> OnStage1ButtonClicked =>
            stage1Button.OnClickAsObservable();
        
        public Observable<Unit> OnStage2ButtonClicked =>
            stage2Button.OnClickAsObservable();
        
        public Observable<Unit> OnStage3ButtonClicked =>
            stage3Button.OnClickAsObservable();
        
        public Observable<Unit> OnStage4ButtonClicked =>
            stage4Button.OnClickAsObservable();
        
        public Observable<Unit> OnStage5ButtonClicked =>
            stage5Button.OnClickAsObservable();

        /// <summary>
        /// 選択可能なステージセレクトボタンを表示
        /// </summary>
        /// <param name="currentProgress">選択可能な最大のボタン</param>
        public void ShowStageSelectButtons(int currentProgress)
        {
            stage0Button.interactable = currentProgress >= 0;
            stage1Button.interactable = currentProgress >= 1;
            stage2Button.interactable = currentProgress >= 2;
            stage3Button.interactable = currentProgress >= 3;
            stage4Button.interactable = currentProgress >= 4;
            stage5Button.interactable = currentProgress >= 5;
        }
    }
}
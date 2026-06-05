using R3;
using UnityEngine;
using UnityEngine.UI;

namespace RePuzzleKnights.Scripts.Presentation.Title
{
    public class TitleView : MonoBehaviour
    {
        [Header("Button UI")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        
        public Observable<Unit> OnStartButtonClicked => startButton.OnClickAsObservable();
        public Observable<Unit> OnSettingsButtonClicked => settingsButton.OnClickAsObservable();
        public Observable<Unit> OnQuitButtonClicked => quitButton.OnClickAsObservable();

        /// <summary>
        /// インタラクトの可否を設定する
        /// </summary>
        /// <param name="interactable">インタラクトできるようにするかどうか</param>
        public void SetInteractable(bool interactable)
        {
            if (startButton) 
                startButton.interactable = interactable;
            
            if (quitButton) 
                quitButton.interactable = interactable;
            
            if (settingsButton)
                settingsButton.interactable = interactable;
        }
    }
}



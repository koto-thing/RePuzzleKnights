using R3;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace RePuzzleKnights.Scripts.Title.ButtonControl
{
    public class ButtonView : MonoBehaviour
    {
        [Header("ボタンUI")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        public Observable<Unit> OnStartButtonClicked =>
            startButton.OnClickAsObservable();

        public Observable<Unit> OnSettingsButtonClicked =>
            settingsButton.OnClickAsObservable();

        public Observable<Unit> OnQuitButtonClicked =>
            quitButton.OnClickAsObservable();
    }
}
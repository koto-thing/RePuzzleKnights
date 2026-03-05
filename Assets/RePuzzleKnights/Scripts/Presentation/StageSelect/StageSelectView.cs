using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using R3;
using RePuzzleKnights.Scripts.Domain.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RePuzzleKnights.Scripts.Presentation.StageSelect
{
    public class StageSelectView : MonoBehaviour
    {
        [Header("BackToTitle")]
        [SerializeField] private Button backToTitleButton;
        
        [Header("Stage Selection")]
        [SerializeField] private List<Button> stageButtons;

        [Header("Detail Panel")]
        [SerializeField] private GameObject stageDescriptionPanel;
        [SerializeField] private Button startButton;
        [SerializeField] private Button backButton;
        
        [SerializeField] private TextMeshProUGUI stageTitleText;
        [SerializeField] private TextMeshProUGUI stageDescriptionText;
        [SerializeField] private Image stageImage;
        
        public Observable<Unit> OnBackToTitleButtonClicked => backToTitleButton.OnClickAsObservable();

        private readonly Subject<int> _onStageButtonClicked = new();
        private readonly Subject<Unit> _onStartButtonClicked = new();
        private readonly Subject<Unit> _onBackButtonClicked = new();

        public Observable<int> OnStageButtonClicked => _onStageButtonClicked;
        public Observable<Unit> OnStartButtonClicked => _onStartButtonClicked;
        public Observable<Unit> OnBackButtonClicked => _onBackButtonClicked;

        private void Awake()
        {
            SetupButtonListeners();
        }

        private List<Stage> _currentStages;

        private void SetupButtonListeners()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(() => _onStartButtonClicked.OnNext(Unit.Default));
            }
            
            if (backButton != null)
            {
                backButton.onClick.AddListener(() =>
                {
                    _onBackButtonClicked.OnNext(Unit.Default);
                    HideStageSelectPanel();
                });
            }
            
            for (int i = 0; i < stageButtons.Count; i++)
            {
                int buttonIndex = i;
                stageButtons[i].onClick.AddListener(() =>
                {
                    int targetId = buttonIndex;
                    if (_currentStages != null && _currentStages.All(s => s.Id >= 1))
                    {
                        targetId = buttonIndex + 1;
                    }
                    
                    _onStageButtonClicked.OnNext(targetId);
                });
            }
        }

        public void SetStageList(IEnumerable<Stage> stages)
        {
            if (stages == null)
            {
                Debug.LogWarning("[StageSelectView] SetStageList: stages is null");
                return;
            }
            
            _currentStages = stages.ToList();
            bool startFromOne = _currentStages.All(s => s.Id >= 1);
            
            foreach (var stage in _currentStages)
            {
                int buttonIndex = startFromOne ? stage.Id - 1 : stage.Id;
                
                if (buttonIndex >= 0 && buttonIndex < stageButtons.Count)
                {
                    bool isLocked = !stage.IsUnlocked;
                    stageButtons[buttonIndex].interactable = !isLocked;
                }
                else
                {
                    Debug.LogWarning($"[StageSelectView] Stage ID {stage.Id} maps to button index {buttonIndex} which is out of range (buttons: 0-{stageButtons.Count - 1})");
                }
            }
        }

        public void SetSelectedStage(Stage stage, Sprite image)
        {
            if (stage == null)
            {
                Debug.LogWarning("[StageSelectView] SetSelectedStage: stage is null");
                return;
            }
            
            ShowStageSelectPanel();

            if (stageTitleText != null) 
                stageTitleText.text = stage.Name;
            
            if (stageDescriptionText != null) 
                stageDescriptionText.text = stage.Description;
            
            if (stageImage != null) 
                stageImage.sprite = image;
        }

        public void SetInteractable(bool interactable)
        {
            var canvasGroup = stageDescriptionPanel?.GetComponent<CanvasGroup>();
            if (canvasGroup != null) 
            {
                canvasGroup.interactable = interactable;
                canvasGroup.blocksRaycasts = interactable;
            }
            
            if (startButton != null)
                startButton.interactable = interactable;
            
            foreach(var btn in stageButtons) 
                btn.interactable = interactable;
        }

        private void ShowStageSelectPanel()
        {
            if (stageDescriptionPanel == null)
            {
                Debug.LogError("[StageSelectView] stageDescriptionPanel is null!");
                return;
            }
            
            stageDescriptionPanel.SetActive(true);
            stageDescriptionPanel.transform.DOKill();
            
            var pos = stageDescriptionPanel.transform.localPosition;
            stageDescriptionPanel.transform.localPosition = new Vector3(1000, pos.y, pos.z);
            
            stageDescriptionPanel.transform.DOLocalMoveX(0, 0.5f)
                .SetEase(Ease.OutCubic);
        }

        private void HideStageSelectPanel()
        {
            if (stageDescriptionPanel == null)
            {
                Debug.LogError("[StageSelectView] stageDescriptionPanel is null!");
                return;
            }
            
            stageDescriptionPanel.transform.DOKill();
            stageDescriptionPanel.transform.DOLocalMoveX(1000, 0.5f)
                .SetEase(Ease.InCubic)
                .OnComplete(() =>
                {
                    stageDescriptionPanel.SetActive(false);
                });
        }

        private void OnDestroy()
        {
            _onStageButtonClicked?.Dispose();
            _onStartButtonClicked?.Dispose();
            _onBackButtonClicked?.Dispose();
        }
    }
}

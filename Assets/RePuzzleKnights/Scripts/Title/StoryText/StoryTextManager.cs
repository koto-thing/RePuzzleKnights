using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoryTextManager : MonoBehaviour
{
    [Header("テキスト")] 
    [SerializeField] private TextMeshProUGUI nameText;     // キャラクタ名
    [SerializeField] private TextMeshProUGUI sentenceText; // セリフ
    
    [Header("画像")]
    [SerializeField] private SpriteRenderer characterSr; //キャラ画像のSpriteRenderer
    [SerializeField] private GameObject background;

    [Header("ボタン")] 
    [SerializeField] private Button nextButton; // テキストを進めるボタン
    [SerializeField] private Button backButton; // 一つ前に戻るボタン
    [SerializeField] private Button skipButton; // テキストを最後までスキップするボタン
    
    [Header("依存関係")]
    [SerializeField] private ScenarioModel model;
    [SerializeField] private StoryTextLoader storyTextLoader;
    
    /* getter と setter */
    public TextMeshProUGUI NameText     { get => nameText;     set => nameText = value; }
    public TextMeshProUGUI SentenceText { get => sentenceText; set => sentenceText = value; }
    public SpriteRenderer CharacterSr   { get => characterSr; }
    public Button NextButton        { get => nextButton; }
    public Button BackButton        { get => backButton; }
    
    // @brief エントリポイント
    private void Start()
    {
        storyTextLoader = new StoryTextLoader();
        model.Init();
        LoadScenario();
        SubscribeEvents();
    }
    
    // @brief 会話ウィンドウの表示更新
    // @param scenarioData シナリオデータ, currentIndex シナリオのインデックス
    public void UpdateText(ScenarioData scenarioData, int currentIndex)
    {
        Debug.Log("currentSceneIndex: " + currentIndex);
        nextButton.gameObject.SetActive(currentIndex < scenarioData.scenes.Count);
        backButton.gameObject.SetActive(currentIndex != 0);
        skipButton.gameObject.SetActive(currentIndex < scenarioData.scenes.Count - 1);
        NameText.text = scenarioData.scenes[currentIndex].name;
        SentenceText.text = scenarioData.scenes[currentIndex].sentence;

        string path = "CharacterImages/" + scenarioData.scenes[currentIndex].image;
        CharacterSr.sprite = Resources.Load<Sprite>(path);
    }
    
    // @brief イベント群の登録
    private void SubscribeEvents()
    {
        // 会話ウィンドウの表示更新、テキストを次へ進める
        nextButton.onClick
            .AddListener(() =>
            {
                if (model.CurrentSceneIndex < model.ScenarioData.scenes.Count)
                {
                    UpdateText(model.ScenarioData, model.CurrentSceneIndex++);
                }
                else if (model.CurrentFilePathIndex < model.FilePath.Length)
                {
                    model.CurrentSceneIndex = 0;
                    LoadScenario();
                } 
            });
        
        // 会話ウィンドウの表示更新、テキストを前に戻す
        backButton.onClick
            .AddListener(() =>
            {
                if (model.CurrentSceneIndex > 0)
                {
                    Debug.Log("バックボタンが押されました");
                    model.CurrentSceneIndex -= 2;
                    UpdateText(model.ScenarioData, model.CurrentSceneIndex++);
                }
            });
        
        // 会話ウィンドウの表示更新、テキストを最後までスキップする
        skipButton.onClick
            .AddListener(() =>
            {
                if (model.CurrentSceneIndex < model.ScenarioData.scenes.Count)
                {
                    StartCoroutine(SkipText());
                }
            });
    }

    // @brief シナリオ読み込み
    private void LoadScenario()
    {
        model.ScenarioData = storyTextLoader.LoadScenario(model.FilePath[model.CurrentFilePathIndex++]); // シナリオ読み込み
        UpdateText(model.ScenarioData, model.CurrentSceneIndex++);
    }

    //　テキストスキップの演出、テキストを最後まで順に表示する
    private IEnumerator SkipText()
    {
        while (model.CurrentSceneIndex < model.ScenarioData.scenes.Count)
        {
            UpdateText(model.ScenarioData, model.CurrentSceneIndex++);
            yield return new WaitForSeconds(0.05f);
        }
    }
    
}

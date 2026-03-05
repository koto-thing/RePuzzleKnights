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
    [SerializeField] private Sprite[] characterSprites;
    [SerializeField] private Sprite background;

    [Header("ボタン")] 
    [SerializeField] private Button nextButton; // テキストを進めるボタン
    [SerializeField] private Button backButton; // 一つ前に戻るボタン
    
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
        nextButton.enabled = currentIndex != scenarioData.scenes.Count;
        backButton.enabled = currentIndex != 1;
        NameText.text = scenarioData.scenes[currentIndex].name;
        SentenceText.text = scenarioData.scenes[currentIndex].sentence;
        CharacterSr.sprite = characterSprites[int.Parse(scenarioData.scenes[currentIndex].charaImage)];
    }
    
    // @brief イベント群の登録
    private void SubscribeEvents()
    {
        // 会話ウィンドウの表示更新
        nextButton.onClick
            .AddListener(() =>
            {
                Debug.Log("ボタンが押されました");

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
    }

    // @brief シナリオ読み込み
    private void LoadScenario()
    {
        model.ScenarioData = storyTextLoader.LoadScenario(model.FilePath[model.CurrentFilePathIndex++]); // シナリオ読み込み
        UpdateText(model.ScenarioData, model.CurrentSceneIndex++);
    }
}

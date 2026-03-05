using UnityEngine;
using R3;

[CreateAssetMenu(fileName = "ScenarioModel", menuName = "ScriptableObject/ScenarioModel")]
public class ScenarioModel : ScriptableObject
{
    [Header("シナリオファイルへのパス")] 
    [SerializeField] private string[] filePath;
    [SerializeField] private int currentFilePathIndex;
        
    [Header("シナリオデータ")]
    [SerializeField] private ScenarioData scenarioData;
    [SerializeField] private int currentSceneIndex;

    [Header("次のシーン")] 
    [SerializeField] private string nextScene;
        
    [Header("シナリオが終了したかどうか")]
    [SerializeField] private ReactiveProperty<bool> isScenarioEnd = new ReactiveProperty<bool>(false);

    /* getter と setter */
    public string[] FilePath => filePath;
    public int CurrentFilePathIndex { get => currentFilePathIndex; set => currentFilePathIndex = value; }
    public ScenarioData ScenarioData { get => scenarioData; set => scenarioData = value; }
    public int CurrentSceneIndex { get => currentSceneIndex; set => currentSceneIndex = value; }
    public string NextScene { get => nextScene; set => nextScene = value; }
    public Observable<bool> IsScenarioEnd => isScenarioEnd.AsObservable();

    public void Init()
    {
        currentSceneIndex = 0;
        currentFilePathIndex = 0;
    }
        
    public void CheckScenarioEnd()
    {
        if (currentSceneIndex >= scenarioData.scenes.Count)
        {
            currentFilePathIndex++;
            currentSceneIndex = 0;
            isScenarioEnd.Value = true;
        }
        else
        {
            isScenarioEnd.Value = false;
        }
    }
}

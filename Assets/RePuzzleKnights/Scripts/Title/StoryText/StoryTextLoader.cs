using System.IO;
using UnityEngine;

public class StoryTextLoader
{
    // @brief シナリオ読み込み
    public ScenarioData LoadScenario(string filePath)
    {
        ScenarioData scenarioData = null;
        string path = Path.Combine(Application.streamingAssetsPath, filePath);
        string jsonText = "";
            
        if (File.Exists(path))
        {
            jsonText = File.ReadAllText(path);
            scenarioData = JsonUtility.FromJson<ScenarioData>(jsonText);
            Debug.Log("シナリオをロードしました: " + scenarioData.scenes.Count + " 件");

            foreach (var scene in scenarioData.scenes)
            {
                Debug.Log(scene.id + ": " + scene.name + ": " + scene.sentence);
            }
        }
        else
        {
            Debug.LogError("JSONファイルが見つかりません: " + path);
        }

        return scenarioData;
    }
}

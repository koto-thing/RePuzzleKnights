using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RePuzzleKnights.Scripts.StageSelect.StageSelect
{
    /// <summary>
    /// ステージの詳細情報を定義するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Stage Data", menuName = "StageSelectSystem/Crate Stage Data")]
    public class StageDescriptionDataSO : ScriptableObject
    {
        [SerializeField] private int stageNumber;
        [SerializeField] private string stageName;
        [SerializeField, TextArea] private string stageDescription;
        [SerializeField] private Sprite stageImage;
        [SerializeField] private AssetReference sceneRef;
        
        public int StageNumber => stageNumber;
        public string StageName => stageName;
        public string StageDescription => stageDescription;
        public Sprite Image => stageImage;
        public AssetReference SceneRef => sceneRef;
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RePuzzleKnights.Scripts.StageSelect.StageSelect
{
    [CreateAssetMenu(fileName = "New Stage Select Data", menuName = "StageSelectSystem/Crate Stage Select Data")]
    public class StageSelectDataSO : ScriptableObject
    {
        private const int maxProgress = 5;
        
        [SerializeField] private int currentProgress = 0;
        [SerializeField] private List<AssetReference> stageDescriptionDataRefs;
        
        public int CurrentProgress
        {
            get => currentProgress;
            set => currentProgress = Mathf.Clamp(value, 0, maxProgress);
        }
        
        public List<AssetReference> StageDescriptionDataRefs => stageDescriptionDataRefs;
    }
}
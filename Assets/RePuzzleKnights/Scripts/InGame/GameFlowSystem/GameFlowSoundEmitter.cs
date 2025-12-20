using FMODUnity;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.GameFlowSystem
{
    public class GameFlowSoundEmitter : MonoBehaviour
    {
        [SerializeField] private StudioEventEmitter bgm;
        [SerializeField] private StudioEventEmitter stageClearSe;
        [SerializeField] private StudioEventEmitter gameOverSe;
        
        public void PlayStageClearSe()
        {
            stageClearSe.Play();
        }
        
        public void PlayGameOverSe()
        {
            gameOverSe.Play();
        }

        public void StopBgm()
        {
            bgm.Stop();
        }
    }
}
using Cysharp.Threading.Tasks;
using RePuzzleKnights.Scripts.Application.Common;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Application.StageSelect
{
    public class StageSelectSceneTransitionUseCase
    {
        private readonly ISceneLoader _sceneLoader;

        private const string TITLE_SCENE = "TitleScene";
        private const string STAGE_0 = "Stage0Scene";
        private const string STAGE_1 = "Stage1Scene";
        private const string STAGE_2 = "Stage2Scene";
        private const string STAGE_3 = "Stage3Scene";
        private const string STAGE_4 = "Stage4Scene";
        private const string STAGE_5 = "Stage5Scene";
        
        public StageSelectSceneTransitionUseCase(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }
        
        /// <summary>
        /// ステージIDに基づいてシーンを遷移
        /// </summary>
        public async UniTask TransitionToStage(int stageId)
        {
            string sceneName = GetSceneName(stageId);
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError($"[StageSelectSceneTransitionUseCase] Invalid stage ID: {stageId}");
                return;
            }
            
            await _sceneLoader.LoadSceneAsync(sceneName);
        }

        private string GetSceneName(int stageId)
        {
            return stageId switch
            {
                0 => STAGE_0,
                1 => STAGE_1,
                2 => STAGE_2,
                3 => STAGE_3,
                4 => STAGE_4,
                5 => STAGE_5,
                _ => null
            };
        }

        public async UniTask TransitionToTitleSceneAsync()
        {
            await _sceneLoader.LoadSceneAsync(TITLE_SCENE);
        }

        public async UniTask TransitionToStage0()
        {
            await _sceneLoader.LoadSceneAsync(STAGE_0);
        }

        public async UniTask TransitionToStage1()
        {
            await _sceneLoader.LoadSceneAsync(STAGE_1);
        }
        
        public async UniTask TransitionToStage2()
        {
            await _sceneLoader.LoadSceneAsync(STAGE_2);
        }
        
        public async UniTask TransitionToStage3()
        {
            await _sceneLoader.LoadSceneAsync(STAGE_3);
        }
        
        public async UniTask TransitionToStage4()
        {
            await _sceneLoader.LoadSceneAsync(STAGE_4);
        }
        
        public async UniTask TransitionToStage5()
        {
            await _sceneLoader.LoadSceneAsync(STAGE_5);
        }
    }
}


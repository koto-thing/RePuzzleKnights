using Cysharp.Threading.Tasks;
using RePuzzleKnights.Scripts.Application.Common;

namespace RePuzzleKnights.Scripts.Application.Title
{
    public class TitleSceneTransitionUseCase
    {
        private readonly ISceneLoader _sceneLoader;

        private const string STAGE_SELECT_SCENE_KEY = "StageSelectScene";

        public TitleSceneTransitionUseCase(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        /// <summary>
        /// ステージ選択画面にシーン遷移する
        /// </summary>
        public async UniTask TransitionToStageSelectSceneAsync()
        {
            await _sceneLoader.LoadSceneAsync(STAGE_SELECT_SCENE_KEY);
        }
    }
}
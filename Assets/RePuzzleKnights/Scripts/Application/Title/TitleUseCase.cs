using R3;

namespace RePuzzleKnights.Scripts.Application.Title
{
    public class TitleUseCase
    {
        public void EndGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                UnityEngine.Application.Quit();
            #endif
        }
    }
}



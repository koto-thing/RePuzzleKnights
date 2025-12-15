using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace RePuzzleKnights.Scripts.StageSelect.ButtonControl
{
    public class ButtonModel
    {
        public async UniTaskVoid OnBackButtonClicked()
        {
            await Addressables.LoadSceneAsync("TitleScene").ToUniTask();
        }
    }
}
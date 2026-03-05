using Cysharp.Threading.Tasks;
using RePuzzleKnights.Scripts.Application.Common;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace RePuzzleKnights.Scripts.Infrastructure.Common
{
    public class AddressablesSceneLoader : ISceneLoader
    {
        private bool _isLoading;
        
        public async UniTask LoadSceneAsync(string key)
        {
            if (_isLoading)
                return;
            
            _isLoading = true;

            try
            {
                await Addressables.LoadSceneAsync(key);
            }
            finally
            {
                _isLoading = false;
            }
        }
    }
}



using RePuzzleKnights.Scripts.Presentation.InGame;
using TMPro;
using UnityEngine;
using VContainer;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame
{
    /// <summary>
    /// 本拠地のステータスを表示するViewクラス
    /// </summary>
    public class BaseStatusView : MonoBehaviour, IBaseStatusView
    {
        [SerializeField] private TextMeshProUGUI durabilityText;
        [SerializeField] private TextMeshProUGUI enemyCountText;
        [SerializeField] private ParticleSystem destroyEffect;
        
        private BaseStatusController _controller;
 
        [Inject]
        public void Construct(BaseStatusController controller)
        {
            this._controller = controller;
        }
 
        public void UpdateDurability(int current, int max)
        {
            if (durabilityText != null)
            {
                durabilityText.text = current.ToString();
            }
        }

        public void UpdateEnemyCount(int defeated, int total)
        {
            if (enemyCountText != null)
            {
                enemyCountText.text = $"ENEMY  {defeated}/{total}";
            }
        }
 
        public void PlayDestroyEffect()
        {
            if (destroyEffect != null)
            {
                destroyEffect.Play();
            }
        }
    }
}


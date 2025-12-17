using R3;
using RePuzzleKnights.Scripts.InGame.Allies.SO;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace RePuzzleKnights.Scripts.InGame.PlacementSystem
{
    public class PlacementConnector : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private AllyDataSO allyData;

        private PlacementModel model;

        [Inject]
        public void Construct(PlacementModel model)
        {
            this.model = model;
        }

        private void Start()
        {
            if (model == null)
            {
                Debug.LogError("PlacementConnector: PlacementModel is null.");
                return;
            }

            model.OnPlacementConfirmed
                .Where(payload => payload.data == allyData)
                .Subscribe(_ => gameObject.SetActive(false))
                .AddTo(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (model == null)
            {
                Debug.LogError("PlacementConnector: PlacementModel is null.");
                return;
            }
            
            model.StartDragging(allyData);
        }
    }
}
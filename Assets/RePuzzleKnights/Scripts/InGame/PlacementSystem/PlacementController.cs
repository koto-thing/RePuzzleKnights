using UnityEngine.InputSystem;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.InGame.PlacementSystem
{
    public class PlacementController : ITickable
    {
        private readonly PlacementModel model;

        public PlacementController(PlacementModel model)
        {
            this.model = model;
        }

        public void Tick()
        {
            if (Mouse.current?.rightButton.wasPressedThisFrame == true)
            {
                model.Cancel();
                return;
            }

            if (model.CurrentPlacementState.CurrentValue == Enum.PlacementState.IDLE)
                return;
            
            switch (model.CurrentPlacementState.CurrentValue)
            {
                case Enum.PlacementState.DRAGGING:
                    model.HandleDragging();
                    break;

                case Enum.PlacementState.ORIENTING:
                    model.HandleOrienting();
                    break;
            }
        }
    }
}


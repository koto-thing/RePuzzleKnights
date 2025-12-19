using R3;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Allies
{
    public class AllyView : MonoBehaviour
    {
        [SerializeField] private Transform modelTransform;
        
        public Vector3 FacingDirection { get; private set; } = Vector3.forward;

        public void LookAtSnap(Vector3 targetPoint)
        {
            Vector3 diff = targetPoint - modelTransform.position;
            diff.y = 0;
            if (diff.sqrMagnitude < 0.001f)
                return;

            Vector3 direction;
            float angle;
            if (Mathf.Abs(diff.x) > Mathf.Abs(diff.z))
            {
                if (diff.x > 0)
                {
                    direction = Vector3.right;
                    angle = 90.0f;
                }
                else
                {
                    direction = Vector3.left;
                    angle = -90.0f;
                }
            }
            else
            {
                if (diff.z > 0)
                {
                    direction = Vector3.forward;
                    angle = 0.0f;
                }
                else
                {
                    direction = Vector3.back;
                    angle = 180.0f;
                }
            }

            FacingDirection = direction;
            modelTransform.rotation = Quaternion.Euler(0, angle, 0);
        }

        public void InitializeStatusDisplay(AllyBattleModel model, float maxHp)
        {
            var statusView = GetComponentInChildren<AllyStatusView>();
            if (statusView != null)
            {
                statusView.SetMaxHp(maxHp);
                
                model.CurrentHp
                    .Subscribe(hp => statusView.UpdateHp(hp))
                    .AddTo(this);
            }
        }
    }
}
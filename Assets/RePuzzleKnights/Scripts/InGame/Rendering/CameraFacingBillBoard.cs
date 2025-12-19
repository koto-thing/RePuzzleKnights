using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Rendering
{
    public class CameraFacingBillBoard : MonoBehaviour
    {
        private Camera targetCamera;

        private void Start()
        {
            if (Camera.main != null)
                targetCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (targetCamera == null)
                return;

            transform.rotation = targetCamera.transform.rotation;
        }
    }
}
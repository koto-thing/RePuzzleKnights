using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.Rendering
{
    public class CameraFacingBillBoard : MonoBehaviour
    {
        private Camera _targetCamera;

        private void Start()
        {
            if (Camera.main != null)
                _targetCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (_targetCamera == null)
                return;

            transform.rotation = _targetCamera.transform.rotation;
        }
    }
}



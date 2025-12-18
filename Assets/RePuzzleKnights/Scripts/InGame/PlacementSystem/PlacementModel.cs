using R3;
using RePuzzleKnights.Scripts.InGame.Allies.Enum;
using RePuzzleKnights.Scripts.InGame.Allies.SO;
using RePuzzleKnights.Scripts.InGame.PlacementSystem.Enum;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RePuzzleKnights.Scripts.InGame.PlacementSystem
{
    public class PlacementModel
    {
        private readonly LayerMask placementLayerMask = LayerMask.GetMask("Default", "Ground", "HighGround");
        private const string GroundTag = "GROUND_BLOCK";
        private const string HighGroundTag = "HIGHGROUND_BLOCK";
        
        public ReadOnlyReactiveProperty<PlacementState> CurrentPlacementState => currentPlacementState;
        private readonly ReactiveProperty<PlacementState> currentPlacementState = new (PlacementState.IDLE);

        public ReadOnlyReactiveProperty<AllyDataSO> SelectedAlly => selectedAlly;
        private readonly ReactiveProperty<AllyDataSO> selectedAlly = new ();

        public ReadOnlyReactiveProperty<Vector3> PreviewPosition => previewPosition;
        private readonly ReactiveProperty<Vector3> previewPosition = new ();

        public ReadOnlyReactiveProperty<Quaternion> PreviewRotation => previewRotation;
        private readonly ReactiveProperty<Quaternion> previewRotation = new ();

        public ReadOnlyReactiveProperty<bool> IsValidPosition => isValidPosition;
        private readonly ReactiveProperty<bool> isValidPosition = new (false);

        public Observable<(AllyDataSO data, Vector3 position, Quaternion rotation)> OnPlacementConfirmed => onPlacementConfirmed;
        private readonly Subject<(AllyDataSO, Vector3, Quaternion)> onPlacementConfirmed = new();

        public Observable<Unit> OnCanceled => onCanceled;
        private readonly Subject<Unit> onCanceled = new();
        
        public void StartDragging(AllyDataSO data)
        {
            if (currentPlacementState.Value != PlacementState.IDLE)
                return;

            selectedAlly.Value = data;
            isValidPosition.Value = true;
            currentPlacementState.Value = PlacementState.DRAGGING;
        }

        public void HandleDragging()
        {
            if (Camera.main == null)
                return;

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, placementLayerMask);
            if (hit)
            {
                GameObject hitObj = hitInfo.collider.gameObject;
                bool isValid = CheckTagValidity(hitObj);

                Vector3 finalPosition = hitInfo.point + new Vector3(0.0f, 0.5f, 0.0f);
                if (isValid)
                {
                    finalPosition.x = hitObj.transform.position.x;
                    finalPosition.z = hitObj.transform.position.z;
                    finalPosition.y = hitInfo.collider.bounds.max.y;
                }
                
                UpdatePosition(finalPosition, isValid);
            }
            else
            {
                UpdatePosition(Vector3.zero, false);
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryFixPosition();
            }
        }

        public void HandleOrienting()
        {
            if (Camera.main == null)
                return;
            
            // 左クリック長押し中だけ、向きを更新する
            if (Mouse.current.leftButton.isPressed)
            {
                Vector3 centerPos = PreviewPosition.CurrentValue;

                Plane plane = new Plane(Vector3.up, centerPos);
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (plane.Raycast(ray, out float enter))
                {
                    Vector3 mousePoint = ray.GetPoint(enter);
                    Vector3 direction = mousePoint - centerPos;

                    Quaternion rotation = CalculateSnapRotation(direction);
                    UpdateRotation(rotation);
                }
            }
            
            // 左クリックを離した瞬間に確定する
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                ConfirmPlacement();
            }
        }

        private Quaternion CalculateSnapRotation(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.001f)
                return Quaternion.identity;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                return direction.x > 0 ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, -90, 0);
            }
            else
            {
                return direction.z > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
            }
        }

        private void UpdatePosition(Vector3 position, bool isValid)
        {
            if (currentPlacementState.Value != PlacementState.DRAGGING)
                return;

            previewPosition.Value = position;
            isValidPosition.Value = isValid;
        }

        private void TryFixPosition()
        {
            if (currentPlacementState.Value != PlacementState.DRAGGING)
                return;

            if (isValidPosition.Value)
            {
                currentPlacementState.Value = PlacementState.ORIENTING;
            }
            else
            {
                Cancel();
            }
        }

        private void UpdateRotation(Quaternion rotation)
        {
            if (currentPlacementState.Value != PlacementState.ORIENTING)
                return;
            
            previewRotation.Value = rotation;
        }

        private void ConfirmPlacement()
        {
            if (currentPlacementState.Value != PlacementState.ORIENTING)
                return;
            
            onPlacementConfirmed.OnNext((selectedAlly.Value, previewPosition.Value, previewRotation.Value));
            Reset();
        }

        private bool CheckTagValidity(GameObject target)
        {
            var data = selectedAlly.Value;
            if (data == null)
                return false;
            
            if (data.AllyType == AllyType.Ground)
                return target.CompareTag(GroundTag);
            
            if (data.AllyType == AllyType.HighGround)
                return target.CompareTag(HighGroundTag);

            return false;
        }

        public void Cancel()
        {
            onCanceled.OnNext(Unit.Default);
            Reset();
        }

        private void Reset()
        {
            currentPlacementState.Value = PlacementState.IDLE;
            selectedAlly.Value = null;
            isValidPosition.Value = false;
        }
    }
}
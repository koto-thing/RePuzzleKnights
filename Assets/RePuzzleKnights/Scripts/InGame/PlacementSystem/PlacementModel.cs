using R3;
using RePuzzleKnights.Scripts.InGame.Allies;
using RePuzzleKnights.Scripts.InGame.Allies.Enum;
using RePuzzleKnights.Scripts.InGame.Allies.SO;
using RePuzzleKnights.Scripts.InGame.PlacementSystem.Enum;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.PlacementSystem
{
    /// <summary>
    /// 配置システムのデータと状態を管理するModelクラス
    /// 配置状態、選択中の味方、プレビュー位置などを管理
    /// </summary>
    public class PlacementModel
    {
        private readonly LayerMask placementLayerMask = LayerMask.GetMask("Ground", "HighGround");
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

        public Observable<AllyDataSO> OnAllyDefeated => onAllyDefeated;
        private readonly Subject<AllyDataSO> onAllyDefeated = new ();
        
        /// <summary>
        /// ドラッグを開始
        /// </summary>
        public void StartDragging(AllyDataSO data)
        {
            if (currentPlacementState.Value != PlacementState.IDLE)
                return;

            selectedAlly.Value = data;
            isValidPosition.Value = true;
            currentPlacementState.Value = PlacementState.DRAGGING;
        }

        /// <summary>
        /// ドラッグ中の処理
        /// </summary>
        public void HandleDragging(Ray ray)
        {
            bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, placementLayerMask);
            if (hit)
            {
                GameObject hitObj = hitInfo.collider.gameObject;
                bool isTagValid = CheckTagValidity(hitObj);

                Vector3 finalPosition = hitInfo.point + new Vector3(0.0f, 0.5f, 0.0f);
                if (isTagValid)
                {
                    finalPosition.x = hitObj.transform.position.x;
                    finalPosition.z = hitObj.transform.position.z;
                    finalPosition.y = hitInfo.collider.bounds.max.y;
                }

                bool isOccupied = IsPositionOccupied(finalPosition);
                bool isValid = isTagValid && !isOccupied;
                
                UpdatePosition(finalPosition, isValid);
            }
            else
            {
                UpdatePosition(Vector3.zero, false);
            }
        }

        /// <summary>
        /// 向き調整中の処理
        /// </summary>
        public void HandleOrienting(Vector3 mouseWorldPoint)
        {
            Vector3 centerPos = PreviewPosition.CurrentValue;
            Vector3 direction = mouseWorldPoint - centerPos;

            Quaternion rotation = CalculateSnapRotation(direction);
            UpdateRotation(rotation);
        }

        /// <summary>
        /// 位置を固定
        /// </summary>
        public void TryFixPosition()
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

        /// <summary>
        /// 配置を確定
        /// </summary>
        public void ConfirmPlacement()
        {
            if (currentPlacementState.Value != PlacementState.ORIENTING)
                return;
            
            onPlacementConfirmed.OnNext((selectedAlly.Value, previewPosition.Value, previewRotation.Value));
            Reset();
        }

        /// <summary>
        /// スナップした向きを計算
        /// </summary>
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

        /// <summary>
        /// プレビュー位置を更新
        /// </summary>
        private void UpdatePosition(Vector3 position, bool isValid)
        {
            if (currentPlacementState.Value != PlacementState.DRAGGING)
                return;

            previewPosition.Value = position;
            isValidPosition.Value = isValid;
        }

        /// <summary>
        /// プレビュー回転を更新
        /// </summary>
        private void UpdateRotation(Quaternion rotation)
        {
            if (currentPlacementState.Value != PlacementState.ORIENTING)
                return;
            
            previewRotation.Value = rotation;
        }

        /// <summary>
        /// タグの妥当性をチェック
        /// </summary>
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

        /// <summary>
        /// 指定座標にすでに味方がいるかどうかを確認する
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private bool IsPositionOccupied(Vector3 position)
        {
            float checkRadius = 0.3f;
            int allyLayer = LayerMask.GetMask("Ally");

            var colliders = Physics.OverlapSphere(position, checkRadius, allyLayer);
            foreach (var col in colliders)
            {
                if (col.TryGetComponent<AllyEntityHolder>(out var holder))
                {
                    float distance = Vector3.Distance(position, col.transform.position);
                    if (distance < 0.5f)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// キャンセル
        /// </summary>
        public void Cancel()
        {
            onCanceled.OnNext(Unit.Default);
            Reset();
        }

        /// <summary>
        /// 状態をリセット
        /// </summary>
        private void Reset()
        {
            currentPlacementState.Value = PlacementState.IDLE;
            selectedAlly.Value = null;
            isValidPosition.Value = false;
        }

        /// <summary>
        /// 味方が倒された際の通知
        /// </summary>
        public void NotifyAllyDefeated(AllyDataSO data)
        {
            onAllyDefeated.OnNext(data);
        }
    }
}
using R3;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Domain.Enums;
using UnityEngine;

// For AllyStats

namespace RePuzzleKnights.Scripts.Application.InGame
{
    public class PlacementUseCase
    {
        private readonly ReadOnlyReactiveProperty<PlacementState> currentPlacementState;
        private readonly ReactiveProperty<PlacementState> _currentPlacementState = new(PlacementState.IDLE);
        
        public ReadOnlyReactiveProperty<PlacementState> CurrentPlacementState => _currentPlacementState;
        
        private readonly ReactiveProperty<AllyStats> _selectedAlly = new(); 
        public ReadOnlyReactiveProperty<AllyStats> SelectedAlly => _selectedAlly;

        private readonly ReactiveProperty<Vector3> _previewPosition = new();
        public ReadOnlyReactiveProperty<Vector3> PreviewPosition => _previewPosition;

        private readonly ReactiveProperty<Quaternion> _previewRotation = new();
        public ReadOnlyReactiveProperty<Quaternion> PreviewRotation => _previewRotation;

        private readonly ReactiveProperty<bool> _isValidPosition = new(false);
        public ReadOnlyReactiveProperty<bool> IsValidPosition => _isValidPosition;
        
        public Observable<(AllyStats stats, Vector3 position, Quaternion rotation)> OnPlacementConfirmed => _onPlacementConfirmed;
        private readonly Subject<(AllyStats, Vector3, Quaternion)> _onPlacementConfirmed = new();

        public Observable<string> OnAllyDefeated => _onAllyDefeated;
        private readonly Subject<string> _onAllyDefeated = new();

        public Observable<Unit> OnCanceled => _onCanceled;
        private readonly Subject<Unit> _onCanceled = new();

        public PlacementUseCase()
        {
        }

        /// <summary>
        /// 味方が死亡したときの処理
        /// </summary>
        /// <param name="allyName">味方の名前</param>
        public void NotifyAllyDefeated(string allyName)
        {
            _onAllyDefeated.OnNext(allyName);
        }

        /// <summary>
        /// ドラッグ開始時の処理
        /// </summary>
        /// <param name="allyStats">味方の状態</param>
        public void StartDragging(AllyStats allyStats)
        {
            if (_currentPlacementState.Value != PlacementState.IDLE) return;
            
            _selectedAlly.Value = allyStats;
            _isValidPosition.Value = true;
            _currentPlacementState.Value = PlacementState.DRAGGING;
        }

        /// <summary>
        /// プレビュー位置の更新
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="isValid">配置可能な場所かどうか</param>
        public void UpdatePreview(Vector3 position, bool isValid)
        {
            if (_currentPlacementState.Value != PlacementState.DRAGGING) return;

            _previewPosition.Value = position;
            _isValidPosition.Value = isValid;
        }

        /// <summary>
        /// 位置を確定し、向き調整フェーズへ移行する
        /// </summary>
        public void TryFixPosition()
        {
            if (_currentPlacementState.Value != PlacementState.DRAGGING) return;

            if (_isValidPosition.Value)
            {
                _currentPlacementState.Value = PlacementState.ORIENTING;
            }
            else
            {
                Cancel();
            }
        }

        /// <summary>
        /// 回転を更新
        /// </summary>
        /// <param name="rotation">回転</param>
        public void UpdateRotation(Quaternion rotation)
        {
            if (_currentPlacementState.Value != PlacementState.ORIENTING) return;
            _previewRotation.Value = rotation;
        }

        /// <summary>
        /// 配置を確定させる
        /// </summary>
        public void ConfirmPlacement()
        {
            if (_currentPlacementState.Value != PlacementState.ORIENTING) return;

            _onPlacementConfirmed.OnNext((_selectedAlly.Value, _previewPosition.Value, _previewRotation.Value));
            Reset();
        }

        /// <summary>
        /// 配置キャンセル
        /// </summary>
        public void Cancel()
        {
            _onCanceled.OnNext(Unit.Default);
            Reset();
        }

        /// <summary>
        /// 配置リセット
        /// </summary>
        private void Reset()
        {
            _currentPlacementState.Value = PlacementState.IDLE;
            _selectedAlly.Value = null;
            _isValidPosition.Value = false;
        }
    }
}



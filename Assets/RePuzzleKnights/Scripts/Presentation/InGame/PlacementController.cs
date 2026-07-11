using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Domain.Enums;
using RePuzzleKnights.Scripts.Domain.Services;
using RePuzzleKnights.Scripts.Infrastructure.InGame;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Placement;
using UnityEngine;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    /// <summary>
    /// 配置コントローラー
    /// ドラッグアンドドロップで位置決定 → ドラッグアンドドロップで向き決定
    /// </summary>
    public class PlacementController : ITickable
    {
        private readonly PlacementUseCase _useCase;
        private readonly FusionUseCase _fusionUseCase;
        private readonly PlacementInputService _inputService;
        private readonly IPlacementValidator _validator;
        private readonly AllyFactory _allyFactory;
        private readonly IPlacementView _view;
        private readonly PlacementPresenter _presenter;
        private readonly SoulUseCase _soulUseCase;
        
        private readonly LayerMask _placementLayerMask = LayerMask.GetMask("Ground", "HighGround");

        public PlacementController(
            PlacementUseCase useCase,
            FusionUseCase fusionUseCase,
            PlacementInputService inputService,
            IPlacementValidator validator,
            AllyFactory allyFactory,
            IPlacementView view,
            PlacementPresenter presenter,
            SoulUseCase soulUseCase)
        {
            this._useCase = useCase;
            this._fusionUseCase = fusionUseCase;
            this._inputService = inputService;
            this._validator = validator;
            this._allyFactory = allyFactory;
            this._view = view;
            this._presenter = presenter;
            this._soulUseCase = soulUseCase;
        }

        public void StartPlacement(AllyDataSO data)
        {
            if (data == null) 
                return;

            if (!_soulUseCase.CanConsumeSoul(data.Element, 1))
            {
                Debug.LogWarning($"[PlacementController] Not enough Soul to place {data.AllyName}. Required: 1 {data.Element}, Current: {_soulUseCase.GetSoulCount(data.Element).CurrentValue}");
                return;
            }
            
            _view.SetPreviewPrefab(data.PrefabRef);
            _presenter.SetCurrentAllyData(data);
            var stats = _allyFactory.CreateStats(data);
            _useCase.StartDragging(stats);
        }

        public void Tick()
        {
            if (_useCase.CurrentPlacementState.CurrentValue == PlacementState.IDLE) 
                return;

            if (_inputService.IsCancelPressed())
            {
                _useCase.Cancel();
                return;
            }

            if (_useCase.CurrentPlacementState.CurrentValue == PlacementState.DRAGGING)
            {
                HandleDragging();
            }
            else if (_useCase.CurrentPlacementState.CurrentValue == PlacementState.ORIENTING)
            {
                HandleOrienting();
            }
        }

        private void HandleDragging()
        {
            // ドラッグ中は常にマウス位置を追跡
            Ray ray = _inputService.GetScreenRay(_inputService.GetMousePosition());
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, _placementLayerMask))
            {
                 GameObject hitObj = hitInfo.collider.gameObject;
                 Vector3 finalPosition = hitInfo.point + new Vector3(0.0f, 0.5f, 0.0f);
                 
                 finalPosition.x = hitObj.transform.position.x;
                 finalPosition.z = hitObj.transform.position.z;
                 finalPosition.y = hitInfo.collider.bounds.max.y;
                 
                 var allyStats = _useCase.SelectedAlly.CurrentValue;
                 bool isHighGround = allyStats is { PlacementType: PlacementType.HighGround };
                 
                 bool isTerrainValid = _validator.IsTerrainValid(finalPosition, hitObj.tag, isHighGround);
                 
                 GameObject targetAllyObj = _validator.GetAllyObjectAtPosition(finalPosition);
                 bool isFusionPossible = false;
                 
                 if (targetAllyObj != null)
                 {
                     var reference = targetAllyObj.GetComponentInParent<AllyReference>();
                     if (reference != null)
                     {
                         // ドラッグ中のユニットは常にレベル1として扱う
                         var draggingAlly = new Ally("temp", allyStats); 
                         isFusionPossible = _fusionUseCase.CanFuse(reference.Ally, draggingAlly);

                         // 最終進化済みのキャラクターは融合不可 → 占有マスとして扱い配置不可にする
                         if (reference.Ally.FusionState.IsEvolved)
                         {
                             isFusionPossible = false;
                             targetAllyObj = null; // 配置先として無効扱い（赤表示）
                         }
                     }
                 }
                 
                 bool isOccupied = targetAllyObj != null;
                 
                 _useCase.UpdatePreview(finalPosition, isTerrainValid && !isOccupied, isFusionPossible, targetAllyObj);
            }
            else
            {
                _useCase.UpdatePreview(Vector3.zero, false);
            }
            
            // ドラッグ終了時に位置確定
            if (_inputService.IsLeftClickReleased())
            {
                _useCase.TryFixPosition();
            }
        }

        private void HandleOrienting()
        {
            // 向き決定中はマウス位置から向きを計算
            Vector3 mousePos = _inputService.GetMousePosition();
            Ray ray = _inputService.GetScreenRay(mousePos);
            Plane plane = new Plane(Vector3.up, _useCase.PreviewPosition.CurrentValue);
            
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 worldPoint = ray.GetPoint(enter);
                Vector3 direction = worldPoint - _useCase.PreviewPosition.CurrentValue;
                
                Quaternion rotation = CalculateSnapRotation(direction);
                _useCase.UpdateRotation(rotation);
            }
            
            // マウスボタンを離した時に配置確定
            if (_inputService.IsLeftClickReleased())
            {
                _useCase.ConfirmPlacement();
            }
        }

        private Quaternion CalculateSnapRotation(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.001f) return Quaternion.identity;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                return direction.x > 0 ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, -90, 0);
            }
            else
            {
                return direction.z > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
            }
        }
    }
}

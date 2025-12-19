using UnityEngine;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.InGame.PlacementSystem
{
    /// <summary>
    /// 配置システムのコントローラークラス
    /// 入力を受け取り、Modelを更新するビジネスロジックを担当
    /// </summary>
    public class PlacementController : ITickable
    {
        private readonly PlacementModel model;
        private readonly PlacementInputService inputService;

        public PlacementController(PlacementModel model, PlacementInputService inputService)
        {
            this.model = model;
            this.inputService = inputService;
        }

        public void Tick()
        {
            // 右クリックでキャンセル
            if (inputService.IsRightClickPressed())
            {
                model.Cancel();
                return;
            }

            // IDLE状態では何もしない
            if (model.CurrentPlacementState.CurrentValue == Enum.PlacementState.IDLE)
                return;
            
            switch (model.CurrentPlacementState.CurrentValue)
            {
                case Enum.PlacementState.DRAGGING:
                    HandleDragging();
                    break;

                case Enum.PlacementState.ORIENTING:
                    HandleOrienting();
                    break;
            }
        }

        /// <summary>
        /// ドラッグ中の処理
        /// </summary>
        private void HandleDragging()
        {
            // マウス位置からRayを生成してModelに渡す
            var mousePos = inputService.GetMousePosition();
            var ray = inputService.GetScreenRay(mousePos);
            
            model.HandleDragging(ray);

            // 左クリックで位置を固定
            if (inputService.IsLeftClickPressed())
            {
                model.TryFixPosition();
            }
        }

        /// <summary>
        /// 向き調整中の処理
        /// </summary>
        private void HandleOrienting()
        {
            // 左クリック長押し中は向きを更新
            if (inputService.IsLeftClickHeld())
            {
                Vector3 centerPos = model.PreviewPosition.CurrentValue;
                Plane plane = new Plane(Vector3.up, centerPos);
                
                var mousePos = inputService.GetMousePosition();
                var ray = inputService.GetScreenRay(mousePos);
                
                if (inputService.RaycastPlane(plane, ray, out float enter))
                {
                    Vector3 mousePoint = ray.GetPoint(enter);
                    model.HandleOrienting(mousePoint);
                }
            }
            
            // 左クリックを離したら確定
            if (inputService.IsLeftClickReleased())
            {
                model.ConfirmPlacement();
            }
        }
    }
}

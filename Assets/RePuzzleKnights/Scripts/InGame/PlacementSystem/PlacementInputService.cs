using UnityEngine;
using UnityEngine.InputSystem;

namespace RePuzzleKnights.Scripts.InGame.PlacementSystem
{
    /// <summary>
    /// 配置システムの入力を管理するサービスクラス
    /// Input Systemからの入力を抽象化して提供
    /// </summary>
    public class PlacementInputService
    {
        /// <summary>
        /// マウスの現在位置を取得
        /// </summary>
        public Vector2 GetMousePosition()
        {
            return Mouse.current?.position.ReadValue() ?? Vector2.zero;
        }

        /// <summary>
        /// 左クリックが押された瞬間かどうか
        /// </summary>
        public bool IsLeftClickPressed()
        {
            return Mouse.current?.leftButton.wasPressedThisFrame ?? false;
        }

        /// <summary>
        /// 左クリックが押されているかどうか
        /// </summary>
        public bool IsLeftClickHeld()
        {
            return Mouse.current?.leftButton.isPressed ?? false;
        }

        /// <summary>
        /// 左クリックが離された瞬間かどうか
        /// </summary>
        public bool IsLeftClickReleased()
        {
            return Mouse.current?.leftButton.wasReleasedThisFrame ?? false;
        }

        /// <summary>
        /// 右クリックが押された瞬間かどうか
        /// </summary>
        public bool IsRightClickPressed()
        {
            return Mouse.current?.rightButton.wasPressedThisFrame ?? false;
        }

        /// <summary>
        /// 画面座標からRayを生成
        /// </summary>
        public Ray GetScreenRay(Vector2 screenPosition)
        {
            if (Camera.main == null)
                return new Ray();
            
            return Camera.main.ScreenPointToRay(screenPosition);
        }

        /// <summary>
        /// 平面とのレイキャスト
        /// </summary>
        public bool RaycastPlane(Plane plane, Ray ray, out float enter)
        {
            return plane.Raycast(ray, out enter);
        }
    }
}


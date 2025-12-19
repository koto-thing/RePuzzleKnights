using R3;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Allies
{
    /// <summary>
    /// 味方キャラクターの表示を管理するViewクラス
    /// </summary>
    public class AllyView : MonoBehaviour
    {
        [SerializeField] private Transform modelTransform;
        
        public Vector3 FacingDirection { get; private set; } = Vector3.forward;
        
        /// <summary>
        /// 配置時の向きを設定（2D画像の左右反転）
        /// </summary>
        public void SetInitialDirection(Quaternion rotation)
        {
            if (modelTransform == null)
                return;

            // Y軸回転から向きを判定（90度 = 右、-90度 = 左）
            float yAngle = rotation.eulerAngles.y;
            
            // 右向き（90度付近）の場合はスケールを正常に、左向き（-90度や270度付近）の場合はX反転
            bool isFacingRight = Mathf.Abs(Mathf.DeltaAngle(yAngle, 90f)) < 45f;
            
            Vector3 localScale = modelTransform.localScale;
            localScale.x = isFacingRight ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
            modelTransform.localScale = localScale;
            
            // FacingDirectionも更新
            if (isFacingRight)
            {
                FacingDirection = Vector3.right;
            }
            else
            {
                FacingDirection = Vector3.left;
            }
        }

        /// <summary>
        /// ターゲット方向を向く（スナップ処理、2D画像の左右反転）
        /// </summary>
        public void LookAtSnap(Vector3 targetPoint)
        {
            if (modelTransform == null)
                return;

            Vector3 diff = targetPoint - modelTransform.position;
            diff.y = 0;
            if (diff.sqrMagnitude < 0.001f)
                return;

            // 2D画像なので、左右の向きのみを考慮
            bool shouldFaceRight = diff.x > 0;
            
            Vector3 localScale = modelTransform.localScale;
            localScale.x = shouldFaceRight ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
            modelTransform.localScale = localScale;
            
            FacingDirection = shouldFaceRight ? Vector3.right : Vector3.left;
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
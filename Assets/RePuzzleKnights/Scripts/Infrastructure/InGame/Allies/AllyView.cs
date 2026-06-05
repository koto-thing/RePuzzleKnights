using RePuzzleKnights.Scripts.Presentation.InGame;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.Allies
{
    /// <summary>
    /// 味方キャラクターの表示を管理するViewクラス
    /// CleanArchitecture: Infrastructure layer implementing IAllyView
    /// </summary>
    public class AllyView : MonoBehaviour, IAllyView
    {
        [SerializeField] private Transform modelTransform;
        [SerializeField] private AllyStatusView statusView; 
        [SerializeField] private Animator animator;
        
        public Vector3 Position => transform.position;
        public Vector3 FacingDirection { get; private set; } = Vector3.forward;

        private AllyController _controller;

        public void Initialize(AllyController controller)
        {
            this._controller = controller;
        }

        private void Update()
        {
            if (_controller != null)
            {
                _controller.Tick(Time.deltaTime, transform.position);
            }
        }

        private void Awake()
        {
            if (statusView == null) statusView = GetComponentInChildren<AllyStatusView>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }
        
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
        /// ターゲット方向を向く
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

        public void PlayAttackAnimation()
        {
            if (animator) animator.SetTrigger("Attack");
        }

        public void PlayDamageAnimation()
        {
             if (animator) animator.SetTrigger("Damage");
        }

        public void PlayDieAnimation()
        {
             if (animator) animator.SetTrigger("Die");
        }

        public void UpdateHpBar(float current, float max)
        {
            if (statusView != null)
            {
                statusView.SetMaxHp(max);
                statusView.UpdateHp(current);
            }
        }
    }
}



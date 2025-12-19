using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using RePuzzleKnights.Scripts.InGame.Rendering;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    /// <summary>
    /// 敵キャラクターの表示と演出を管理するViewクラス
    /// DOTweenを使った移動アニメーション、ダメージエフェクト等を担当
    /// </summary>
    public class EnemyView : MonoBehaviour
    {
        private EnemyController controller;
    
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float scaleMultiplier = 1.3f;

        [SerializeField] private DissolveEffect dissolveEffect;
        [SerializeField] private Collider enemyCollider;

        private Sequence moveSequence;

        private void Update()
        {
            controller?.Tick(Time.deltaTime);
        }

        /// <summary>
        /// コントローラーを設定
        /// </summary>
        public void SetController(EnemyController enemyController)
        {
            this.controller = enemyController;
            
            // Holderにコントローラーを登録
            var holder = GetComponent<EnemyEntityHolder>();
            if(holder != null) holder.Initialize(enemyController);
        }

        /// <summary>
        /// ステータス表示の初期化
        /// </summary>
        public void InitializeStatusDisplay(EnemyStatus status, float maxHp)
        {
            var statusView = GetComponentInChildren<EnemyStatusView>(); 
            if (statusView != null)
            {
                statusView.SetMaxHp(maxHp);
                
                // HPの変動を監視してスライダーに反映
                status.CurrentHp
                    .Subscribe(hp => statusView.UpdateHp(hp))
                    .AddTo(this);
            }
        }

        /// <summary>
        /// ダメージエフェクトを再生
        /// </summary>
        public void PlayDamageEffect()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.DOColor(Color.red, 0.1f)
                    .SetLoops(2, LoopType.Yoyo);
            }
        }

        public async UniTask PlayDeathEffectAsync()
        {
            moveSequence?.Kill();

            if (enemyCollider != null)
            {
                enemyCollider.enabled = false;
            }

            if (dissolveEffect != null)
            {
                await dissolveEffect.PlayDissolveEffectAsync();
            }
            else
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            }
        }
        
        /// <summary>
        /// 指定された目標地点まで移動
        /// </summary>
        public void MoveTo(Vector3 target, float moveSpeed, Action onComplete)
        {
            moveSequence?.Kill();

            float distance = Vector3.Distance(transform.position, target);
            if (distance <= 0.001f)
            {
                UniTask.Void(async () =>
                {
                    await UniTask.Yield();
                    onComplete?.Invoke();
                });
                return;
            }

            float duration = distance / moveSpeed;

            moveSequence = DOTween.Sequence();
            
            // 移動
            var moveTween = transform.DOMove(target, duration).SetEase(Ease.Linear);
            
            // スプライトがあれば演出、なければ移動のみ
            if (spriteRenderer != null)
            {
                moveSequence
                    .Append(moveTween)
                    .Join(DOTween.Sequence()
                        .Append(spriteRenderer.transform.DOScaleX(0.1f * scaleMultiplier, duration / 2).SetEase(Ease.InOutSine))
                        .Append(spriteRenderer.transform.DOScaleX(0.1f, duration / 2).SetEase(Ease.InOutSine))
                    );
            }
            else
            {
                // AnimatorもSpriteもない場合のシンプルな移動
                moveSequence.Append(moveTween);
            }

            moveSequence.OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// 移動を一時停止
        /// </summary>
        public void PauseMove()
        {
            moveSequence?.Pause();
        }

        /// <summary>
        /// 移動を再開
        /// </summary>
        public void ResumeMove()
        {
            moveSequence?.Play();
        }

        /// <summary>
        /// 移動を再開（ブロック解除時用）
        /// 現在位置から目標地点へ新しく移動を開始
        /// </summary>
        public void RestartMove(Vector3 target, float moveSpeed, Action onComplete)
        {
            // 既存の移動シーケンスを停止
            moveSequence?.Kill();

            // 現在位置から目標地点までの距離を計算
            float distance = Vector3.Distance(transform.position, target);
            if (distance <= 0.001f)
            {
                UniTask.Void(async () =>
                {
                    await UniTask.Yield();
                    onComplete?.Invoke();
                });
                return;
            }

            // 新しい移動を開始
            float duration = distance / moveSpeed;

            moveSequence = DOTween.Sequence();
            
            var moveTween = transform.DOMove(target, duration).SetEase(Ease.Linear);
            
            if (spriteRenderer != null)
            {
                moveSequence
                    .Append(moveTween)
                    .Join(DOTween.Sequence()
                        .Append(spriteRenderer.transform.DOScaleX(0.1f * scaleMultiplier, duration / 2).SetEase(Ease.InOutSine))
                        .Append(spriteRenderer.transform.DOScaleX(0.1f, duration / 2).SetEase(Ease.InOutSine))
                    );
            }
            else
            {
                moveSequence.Append(moveTween);
            }

            moveSequence.OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// 指定位置にスムーズに移動（ブロック時の位置調整用）
        /// </summary>
        public void MoveToPosition(Vector3 targetPosition, float duration)
        {
            // 既存の移動シーケンスはそのまま
            // 位置調整用の別のTweenを使用
            transform.DOMove(targetPosition, duration).SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// アクターを破棄
        /// </summary>
        public void DestroyActor()
        {
            moveSequence?.Kill();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            moveSequence?.Kill();
        }
    }
}
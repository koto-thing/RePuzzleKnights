using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using RePuzzleKnights.Scripts.InGame.Enemies.Interface;
using UnityEngine;
using PlayerLoopTiming = R3.PlayerLoopTiming;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    /// <summary>
    /// 敵キャラクターの表示と演出を管理するViewクラス
    /// DOTweenを使った移動アニメーション、ダメージエフェクト等を担当
    /// </summary>
    public class EnemyView : MonoBehaviour
    {
        private IEnemyEntity controller;
    
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float scaleMultiplier = 1.3f; // 跳ねる演出用

        private Sequence moveSequence;

        /// <summary>
        /// コントローラーを設定
        /// </summary>
        public void SetController(IEnemyEntity controller)
        {
            this.controller = controller;
            
            // Holderにコントローラーを登録
            var holder = GetComponent<EnemyEntityHolder>();
            if(holder != null) holder.Initialize(controller);
        }

        /// <summary>
        /// ステータス表示の初期化
        /// </summary>
        public void InitializeStatusDisplay(EnemyStatus status, float maxHp)
        {
            var statusView = GetComponentInChildren<BugEnemyStatusView>(); 
            if (statusView != null)
            {
                statusView.UpdateSlider(maxHp);
                
                // HPの変動を監視してスライダーに反映
                status.CurrentHp
                    .Subscribe(hp => statusView.UpdateSlider(hp))
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
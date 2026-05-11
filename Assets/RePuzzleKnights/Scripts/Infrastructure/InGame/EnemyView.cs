using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Rendering;
using RePuzzleKnights.Scripts.Presentation.InGame;
using UnityEngine;
using VContainer;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame
{
    public class EnemyView : MonoBehaviour, IEnemyView
    {
        private EnemyController _controller;
    
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float scaleMultiplier = 1.3f;

        [SerializeField] private DissolveEffect dissolveEffect;
        [SerializeField] private Collider enemyCollider;

        private Sequence _moveSequence;

        [Inject]
        public void Construct(EnemyController controller)
        {
            this._controller = controller;
        }

        private void Update()
        {
            _controller?.Tick(Time.deltaTime);
            
            _controller?.UpdatePosition(transform.position);
        }

        public void MoveTo(Vector3 target, float speed)
        {
            StartMove(target, speed);
        }

        public void SetTargetPosition(Vector3 target, float duration)
        {
            _moveSequence?.Kill();
            _moveSequence = DOTween.Sequence();
            _moveSequence.Append(transform.DOMove(target, duration).SetEase(Ease.OutQuad));
            _moveSequence.OnComplete(OnMoveComplete);
        }

        private void StartMove(Vector3 target, float speed)
        {
             _moveSequence?.Kill();

            float distance = Vector3.Distance(transform.position, target);
            if (distance <= 0.001f)
            {
                OnMoveComplete();
                return;
            }

            float duration = distance / speed;

            _moveSequence = DOTween.Sequence();
            
            var moveTween = transform.DOMove(target, duration).SetEase(Ease.Linear);
            
            if (spriteRenderer != null)
            {
                _moveSequence
                    .Append(moveTween)
                    .Join(DOTween.Sequence()
                        .Append(spriteRenderer.transform.DOScaleX(0.1f * scaleMultiplier, duration / 2).SetEase(Ease.InOutSine))
                        .Append(spriteRenderer.transform.DOScaleX(0.1f, duration / 2).SetEase(Ease.InOutSine))
                    );
            }
            else
            {
                _moveSequence.Append(moveTween);
            }

            _moveSequence.OnComplete(OnMoveComplete);
        }

        private void OnMoveComplete()
        {
             _controller.NotifyArrivedAtTarget();
        }
        
        private EnemyStatusView _cachedStatusView;
        private bool _maxHpInitialized = false;

        public void UpdateHp(float current, float max)
        {
             if (_cachedStatusView == null)
                 _cachedStatusView = GetComponentInChildren<EnemyStatusView>();
             
             if (_cachedStatusView != null)
             {
                 if (!_maxHpInitialized)
                 {
                     _cachedStatusView.SetMaxHp(max);
                     _maxHpInitialized = true;
                 }
                 _cachedStatusView.UpdateHp(current);
             }
        }

        public void PlayDamageEffect()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.DOColor(Color.red, 0.1f)
                    .SetLoops(2, LoopType.Yoyo);
            }
        }

        public void PlayDeathEffect()
        {
             UniTask.Void(async () =>
             {
                 _moveSequence?.Kill();
                 if (enemyCollider != null) enemyCollider.enabled = false;
                 if (dissolveEffect != null) await dissolveEffect.PlayDissolveEffectAsync();
                 else await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

                 DestroyActor();
             });
        }

        public void PlayFallEffect()
        {
            UniTask.Void(async () =>
            {
                _moveSequence?.Kill();
                if (enemyCollider != null) enemyCollider.enabled = false;

                const float duration = 0.35f;
                DOTween.Sequence()
                    .Append(transform.DOMove(transform.position + new Vector3(0, -1f, 0), duration).SetEase(Ease.InQuad))
                    .Join(transform.DOScale(Vector3.zero, duration).SetEase(Ease.InQuad));

                await UniTask.Delay(TimeSpan.FromSeconds(duration));
                DestroyActor();
            });
        }

        public void DestroyActor()
        {
             _moveSequence?.Kill();
             Destroy(gameObject);
        }

        private void OnDestroy()
        {
            _moveSequence?.Kill();
        }
    }
}


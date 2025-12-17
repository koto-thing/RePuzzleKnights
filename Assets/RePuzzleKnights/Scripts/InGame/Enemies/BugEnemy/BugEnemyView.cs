using System;
using DG.Tweening;
using RePuzzleKnights.Scripts.InGame.Enemies.Interface;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Enemies.BugEnemy
{
    public class BugEnemyView : MonoBehaviour, IEnemyEntity
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float scaleMultiplier = 1.3f;

        private Sequence moveSequence;
        private IEnemyEntity controller;
        
        public bool IsFlying => controller?.IsFlying ?? false;
        public bool IsDead => controller?.IsDead ?? false;
        public Vector3 Position => transform.position;
        public float DistanceToGoal => controller?.DistanceToGoal ?? float.MaxValue;
        
        public void SetController(IEnemyEntity enemyController)
        {
            controller = enemyController;
        }
        
        public void OnBlocked(MonoBehaviour blocker)
        {
            controller?.OnBlocked(blocker);
        }
        
        public void OnReleased()
        {
            controller?.OnReleased();
        }
        
        public void TakeDamage(float damage)
        {
            controller?.TakeDamage(damage);
        }

        public void LateUpdate()
        {
            UpdateSpriteRotation();
        }

        /// <summary>
        /// 画像をカメラ方向に向ける
        /// </summary>
        private void UpdateSpriteRotation()
        {
            if (Camera.main != null && spriteRenderer != null)
                spriteRenderer.transform.rotation = Camera.main.transform.rotation;
        }

        /// <summary>
        /// Moves the bug enemy to the specified target position at a given speed, with optional behavior on completion.
        /// </summary>
        /// <param name="target">The position to move the bug enemy to.</param>
        /// <param name="moveSpeed">The speed at which the bug enemy will move towards the target.</param>
        /// <param name="onComplete">The action to be executed upon completing the movement.</param>
        public void MoveTo(Vector3 target, float moveSpeed, Action onComplete)
        {
            moveSequence?.Kill();

            float distance = Vector3.Distance(transform.position, target);
            float duration = distance / moveSpeed;

            moveSequence = DOTween.Sequence();
            moveSequence
                .Append(transform.DOMove(target, duration).SetEase(Ease.Linear))
                .Join(DOTween.Sequence()
                    .Append(spriteRenderer.transform.DOScaleX(0.1f * scaleMultiplier, duration / 2).SetEase(Ease.InOutSine))
                    .Append(spriteRenderer.transform.DOScaleX(0.1f, duration / 2).SetEase(Ease.InOutSine))
                )
                .OnComplete(() => onComplete?.Invoke());
        }
        
        public void PauseMove()
        {
            moveSequence?.Pause();
        }

        public void ResumeMove()
        {
            moveSequence?.Play();
        }

        public void DestroyActor()
        {
            moveSequence?.Kill();
            Destroy(gameObject);
        }
    }
}
using System;
using DG.Tweening;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Enemies.BugEnemy
{
    public class BugEnemyView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float scaleMultiplier = 1.3f;

        private Sequence moveSequence;

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
    }
}
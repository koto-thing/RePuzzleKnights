using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Allies
{
    public class AllyAnimationController : MonoBehaviour
    {
        private Animator animator;
        private float attackAnimationSpeed = 1.0f;

        private static readonly int AttackTrigger = Animator.StringToHash("Attack");
        private static readonly int IsDeadBool = Animator.StringToHash("IsDead");

        private const string NormalStateName = "Normal";

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// 攻撃間隔に基づいてアニメーション速度を設定
        /// </summary>
        /// <param name="attackInterval">攻撃間隔（秒）</param>
        public void SetAttackSpeed(float attackInterval)
        {
            if (animator == null)
                return;

            // 攻撃アニメーションの元の長さを取得
            float baseAnimLength = 1.0f; // デフォルト値
            
            // Attackステートのクリップを探す
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name.Contains("Attack") || clip.name.Contains("attack"))
                {
                    baseAnimLength = clip.length;
                    break;
                }
            }

            // アニメーション速度 = アニメーションの長さ / 攻撃間隔
            // 攻撃間隔が短いほど速く再生される
            attackAnimationSpeed = baseAnimLength / attackInterval;
            
            // 速度が極端にならないように制限
            attackAnimationSpeed = Mathf.Clamp(attackAnimationSpeed, 0.5f, 3.0f);
        }

        public void PlayAttack()
        {
            if (animator != null)
            {
                animator.SetTrigger(AttackTrigger);
                
                // アニメーション速度を設定
                animator.speed = attackAnimationSpeed;
            }
        }

        public void StopAttack()
        {
            if (animator != null)
            {
                animator.ResetTrigger(AttackTrigger);
                animator.speed = 1.0f; // 通常速度に戻す
                animator.Play(NormalStateName);
            }
        }

        public void PlayDeath()
        {
            if (animator != null)
            {
                animator.SetBool(IsDeadBool, true);
            }
        }
        
        /// <summary>
        /// 死亡アニメーションの完了を通知
        /// </summary>
        public UniTask PlayDeathAsync()
        {
            PlayDeath();
        
            // アニメーションの長さを取得して待機
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                float animLength = stateInfo.length;
                return UniTask.Delay(TimeSpan.FromSeconds(animLength), cancellationToken: this.GetCancellationTokenOnDestroy());
            }
        
            return UniTask.CompletedTask;
        }

        public void ResetToNormal()
        {
            if (animator != null)
            {
                animator.SetBool(IsDeadBool, false);
                animator.Play("Normal");
            }
        }
    }
}
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.Allies
{
    public class AllyAnimationController : MonoBehaviour
    {
        private Animator _animator;
        private float _attackAnimationSpeed = 1.0f;

        [SerializeField, Tooltip("斬撃エフェクト")] private VisualEffect slashVfx;

        private static readonly int AttackTrigger = Animator.StringToHash("Attack");
        private static readonly int IsDeadBool = Animator.StringToHash("IsDead");

        private const string NormalStateName = "Normal";

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            
            if (slashVfx != null)
            {
                slashVfx.Stop();
            }
        }

        /// <summary>
        /// 攻撃間隔に基づいてアニメーション速度を設定
        /// </summary>
        /// <param name="attackInterval">攻撃間隔（秒）</param>
        public void SetAttackSpeed(float attackInterval)
        {
            if (_animator == null)
                return;

            // 攻撃アニメーションの元の長さを取得
            float baseAnimLength = 1.0f; // デフォルト値
            
            // Attackステートのクリップを探す
            foreach (var clip in _animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name.Contains("Attack") || clip.name.Contains("attack"))
                {
                    baseAnimLength = clip.length;
                    break;
                }
            }

            // アニメーション速度 = アニメーションの長さ / 攻撃間隔
            _attackAnimationSpeed = baseAnimLength / attackInterval;
            
            // 速度が極端にならないように制限
            _attackAnimationSpeed = Mathf.Clamp(_attackAnimationSpeed, 0.5f, 3.0f);
        }

        public void PlayAttack(Vector3 targetPosition = default)
        {
            if (_animator != null)
            {
                _animator.SetTrigger(AttackTrigger);
                
                // アニメーション速度を設定
                _animator.speed = _attackAnimationSpeed;
            }

            if (slashVfx != null)
            {
                if (targetPosition != default)
                {
                    // カメラの向きに合わせてエフェクトを回転（Billboard）
                    if (Camera.main != null)
                    {
                        slashVfx.transform.rotation = Camera.main.transform.rotation;
                        
                        // カメラ側に少し寄せる（0.2m）ことで地面への埋まり込みを防ぐ
                        slashVfx.transform.position = targetPosition - Camera.main.transform.forward * 0.2f;
                    }
                    else
                    {
                        slashVfx.transform.position = targetPosition;
                    }
                }

                // 斬撃の角度をランダムに設定
                float randomAngle = UnityEngine.Random.Range(0f, 360f);
                slashVfx.SetFloat("SlashAngle", randomAngle);
                
                slashVfx.Play();
            }
        }

        public void StopAttack()
        {
            if (_animator != null)
            {
                _animator.ResetTrigger(AttackTrigger);
                _animator.speed = 1.0f; // 通常速度に戻す
                _animator.Play(NormalStateName);
            }

            if (slashVfx != null)
            {
                slashVfx.Stop();
            }
        }

        public void PlayDeath()
        {
            if (_animator != null)
            {
                _animator.SetBool(IsDeadBool, true);
            }

            if (slashVfx != null)
            {
                slashVfx.Stop();
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
            if (_animator != null)
            {
                _animator.SetBool(IsDeadBool, false);
                _animator.Play("Normal");
            }
        }
    }
}



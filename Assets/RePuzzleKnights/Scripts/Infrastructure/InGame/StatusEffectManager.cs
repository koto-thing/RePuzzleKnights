using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RePuzzleKnights.Scripts.Domain.Enums;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame
{
    /// <summary>
    /// 敵のデバフ（状態異常）ライフサイクルと挙動を管理するインフラコンポーネント。
    /// </summary>
    public class StatusEffectManager : MonoBehaviour
    {
        private class ActiveEffect
        {
            public StatusEffectType Type;
            public float RemainingDuration;
            public float Value;
        }

        private readonly List<ActiveEffect> _activeEffects = new();
        
        private EnemyView _enemyView;
        private EnemyEntityHolder _holder;
        
        private float _burnTimer = 0f;
        private bool _isInitialized = false;

        private GameObject _burnEffectObj;
        private GameObject _slowEffectObj;
        private GameObject _stunEffectObj;

        private bool _isSpawningBurn = false;
        private bool _isSpawningSlow = false;
        private bool _isSpawningStun = false;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_isInitialized) return;
            _enemyView = GetComponent<EnemyView>();
            _holder = GetComponent<EnemyEntityHolder>();
            _isInitialized = true;
        }

        public void ApplyEffect(StatusEffectType type, float duration, float value)
        {
            Initialize();
            
            var existing = _activeEffects.Find(e => e.Type == type);
            if (existing != null)
            {
                existing.RemainingDuration = Mathf.Max(existing.RemainingDuration, duration);
                existing.Value = Mathf.Max(existing.Value, value);
            }
            else
            {
                _activeEffects.Add(new ActiveEffect
                {
                    Type = type,
                    RemainingDuration = duration,
                    Value = value
                });
            }

            UpdateStatusModifiers();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            bool stateChanged = false;

            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];
                effect.RemainingDuration -= dt;

                if (effect.RemainingDuration <= 0f)
                {
                    _activeEffects.RemoveAt(i);
                    stateChanged = true;
                }
            }

            if (stateChanged)
            {
                UpdateStatusModifiers();
            }

            // 火傷継続ダメージ
            var burn = _activeEffects.Find(e => e.Type == StatusEffectType.Burn);
            if (burn != null)
            {
                _burnTimer += dt;
                if (_burnTimer >= 1.0f)
                {
                    _burnTimer -= 1.0f;
                    ApplyBurnDamage(burn.Value);
                }
            }
            else
            {
                _burnTimer = 0f;
            }
        }

        private void ApplyBurnDamage(float damage)
        {
            if (_holder != null && _holder.Entity != null && !_holder.Entity.IsDead)
            {
                _holder.Entity.TakeDamage(damage);
                if (_enemyView != null)
                {
                    _enemyView.PlayDamageEffect();
                }
            }
        }

        private void UpdateStatusModifiers()
        {
            if (_enemyView == null) return;

            float speedMult = 1.0f;
            bool isStunned = false;

            var stun = _activeEffects.Find(e => e.Type == StatusEffectType.Stun);
            if (stun != null)
            {
                isStunned = true;
                speedMult = 0f;
            }

            if (!isStunned)
            {
                var slow = _activeEffects.Find(e => e.Type == StatusEffectType.Slow);
                if (slow != null)
                {
                    speedMult = Mathf.Max(0f, 1.0f - slow.Value);
                }
            }

            _enemyView.SetSpeedMultiplier(speedMult);
            
            var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                if (isStunned)
                {
                    spriteRenderer.color = new Color(1.0f, 0.9f, 0.4f, 1.0f);
                }
                else if (_activeEffects.Exists(e => e.Type == StatusEffectType.Burn))
                {
                    spriteRenderer.color = new Color(1.0f, 0.6f, 0.6f, 1.0f);
                }
                else if (_activeEffects.Exists(e => e.Type == StatusEffectType.Slow))
                {
                    spriteRenderer.color = new Color(0.6f, 0.8f, 1.0f, 1.0f);
                }
                else
                {
                    spriteRenderer.color = Color.white;
                }
            }

            // VFXエフェクトの非同期制御
            bool hasBurn = _activeEffects.Exists(e => e.Type == StatusEffectType.Burn);
            if (hasBurn)
            {
                if (_burnEffectObj == null && !_isSpawningBurn)
                {
                    _isSpawningBurn = true;
                    SpawnBurnVfxAsync().Forget();
                }
            }
            else
            {
                if (_burnEffectObj != null)
                {
                    Destroy(_burnEffectObj);
                    _burnEffectObj = null;
                }
            }

            bool hasSlow = _activeEffects.Exists(e => e.Type == StatusEffectType.Slow);
            if (hasSlow && !isStunned)
            {
                if (_slowEffectObj == null && !_isSpawningSlow)
                {
                    _isSpawningSlow = true;
                    SpawnSlowVfxAsync().Forget();
                }
            }
            else
            {
                if (_slowEffectObj != null)
                {
                    Destroy(_slowEffectObj);
                    _slowEffectObj = null;
                }
            }

            if (isStunned)
            {
                if (_stunEffectObj == null && !_isSpawningStun)
                {
                    _isSpawningStun = true;
                    SpawnStunVfxAsync().Forget();
                }
            }
            else
            {
                if (_stunEffectObj != null)
                {
                    Destroy(_stunEffectObj);
                    _stunEffectObj = null;
                }
            }
        }

        private async UniTaskVoid SpawnBurnVfxAsync()
        {
            try
            {
                var go = await UI.EffectVisualFactory.CreateBurnEffectAsync(transform);
                bool hasBurn = _activeEffects.Exists(e => e.Type == StatusEffectType.Burn);
                if (hasBurn && this != null && gameObject != null)
                {
                    _burnEffectObj = go;
                }
                else
                {
                    Destroy(go);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[StatusEffectManager] Burn VFX error: {ex.Message}");
            }
            finally
            {
                _isSpawningBurn = false;
            }
        }

        private async UniTaskVoid SpawnSlowVfxAsync()
        {
            try
            {
                var go = await UI.EffectVisualFactory.CreateSlowEffectAsync(transform);
                bool hasSlow = _activeEffects.Exists(e => e.Type == StatusEffectType.Slow);
                bool isStunned = _activeEffects.Exists(e => e.Type == StatusEffectType.Stun);
                if (hasSlow && !isStunned && this != null && gameObject != null)
                {
                    _slowEffectObj = go;
                }
                else
                {
                    Destroy(go);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[StatusEffectManager] Slow VFX error: {ex.Message}");
            }
            finally
            {
                _isSpawningSlow = false;
            }
        }

        private async UniTaskVoid SpawnStunVfxAsync()
        {
            try
            {
                var go = await UI.EffectVisualFactory.CreateStunEffectAsync(transform);
                bool isStunned = _activeEffects.Exists(e => e.Type == StatusEffectType.Stun);
                if (isStunned && this != null && gameObject != null)
                {
                    _stunEffectObj = go;
                }
                else
                {
                    Destroy(go);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[StatusEffectManager] Stun VFX error: {ex.Message}");
            }
            finally
            {
                _isSpawningStun = false;
            }
        }

        public float GetDefenseDebuffAmount()
        {
            var defDebuff = _activeEffects.Find(e => e.Type == StatusEffectType.DefDebuff);
            return defDebuff != null ? defDebuff.Value : 0f;
        }
    }
}

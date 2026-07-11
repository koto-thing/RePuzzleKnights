using System;
using Cysharp.Threading.Tasks;
using R3;
using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO;
using RePuzzleKnights.Scripts.Presentation.InGame;
using UnityEngine;
using VContainer;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.Allies
{
    /// <summary>
    /// 味方の戦闘システムを統合するPresenterクラス
    /// Model、Controller、Viewの連携を管理
    /// </summary>
    public class AllyBattlePresenter : MonoBehaviour
    {
        [SerializeField] private AllyView view;
        [SerializeField] private AllyDataSO allyData;
        [SerializeField] private AllyEntityHolder entityHolder;
        [SerializeField] private AllyAnimationController animController;

        private AllyBattleController _controller;
        private AllyBattleModel _model;
        private AllyPresenter _domainPresenter;
        private CompositeDisposable _disposables = new();
        private GameObject _regenEffectObj;
        private bool _isSpawningRegen = false;

        // DI Dependencies
        private FusionUseCase _fusionUseCase;
        private AllyFactory _allyFactory;
        private AddressableAllyDataRepository _allyDataRepository;
        private AllyDetailPresenter _detailPresenter;

        [Inject]
        public void Construct(
            FusionUseCase fusionUseCase, 
            AllyFactory allyFactory,
            AddressableAllyDataRepository allyDataRepository,
            AllyDetailPresenter detailPresenter)
        {
            this._fusionUseCase = fusionUseCase;
            this._allyFactory = allyFactory;
            this._allyDataRepository = allyDataRepository;
            this._detailPresenter = detailPresenter;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize(AllyDataSO data, Action<AllyDataSO> onDeath)
        {
            this.allyData = data;
            // モデルとコントローラーを生成
            _model = new AllyBattleModel(allyData);
            _controller = new AllyBattleController(_model, allyData, transform);
            
            _controller.Initialize(onDeath);
            
            // Initialize View with current HP info
            if (view != null)
            {
                view.UpdateHpBar(allyData.MaxHp, allyData.MaxHp);
            }

            // エンティティホルダーにコントローラーを登録
            if (entityHolder == null)
                entityHolder = GetComponent<AllyEntityHolder>();
            
            if (entityHolder != null)
                entityHolder.Initialize(_controller);
            
            if (animController == null)
                animController = GetComponentInChildren<AllyAnimationController>();

            // アニメーション速度を攻撃間隔に合わせて設定
            if (animController != null)
            {
                animController.SetAttackSpeed(allyData.AttackInterval);
            }

            // イベント購読
            SubscribeEvents();

            // 初期リジェネチェック
            var reference = GetComponent<AllyReference>();
            if (reference != null && reference.Ally != null)
            {
                UpdateRegenEffect(reference.Ally.Stats.SelfRegenPercent);
            }
        }

        /// <summary>
        /// イベント購読の設定
        /// </summary>
        private void SubscribeEvents()
        {
            var reference = GetComponent<AllyReference>();
            if (reference != null && reference.Ally != null)
            {
                reference.Ally.OnStatsUpdated
                    .Subscribe(newStats =>
                    {
                        _controller.UpdateStats(newStats);
                        if (animController != null)
                        {
                            animController.SetAttackSpeed(newStats.AttackInterval);
                        }
                        if (view != null)
                        {
                            view.UpdateHpBar(newStats.MaxHp, newStats.MaxHp);
                        }
                        UpdateRegenEffect(newStats.SelfRegenPercent);
                    })
                    .AddTo(_disposables);
            }

            // 攻撃リクエスト時にViewを更新
            _controller.GetAttackRequestObservable()
                .Subscribe(targets =>
                {
                    if (targets != null && targets.Count > 0 && view != null)
                    {
                        var primaryTarget = targets[0];
                        if (primaryTarget != null)
                        {
                            view.LookAtSnap(primaryTarget.Position);
                        }

                        if (animController != null)
                        {
                            animController.PlayAttack(primaryTarget.Position);
                        }
                    }
                })
                .AddTo(_disposables);

            // 攻撃キャンセル時にアニメーションをリセット
            _controller.GetAttackCancelObservable()
                .Subscribe(_ =>
                {
                    if (animController != null)
                    {
                        animController.StopAttack();
                    }
                })
                .AddTo(_disposables);

            // 死亡時の処理
            _controller.SubscribeDeathEvent(() =>
            {
                HandleDeathAsync().Forget();
            });
            
            // ドメインPresenterの初期化（進化ロジックなどを担当）
            reference = GetComponent<AllyReference>();
            if (reference != null && reference.Ally != null)
            {
                _domainPresenter = new AllyPresenter(
                    reference.Ally, 
                    _fusionUseCase, 
                    view, 
                    _allyFactory, 
                    _allyDataRepository, 
                    gameObject);
                _domainPresenter.Initialize();

                // 詳細パネルへのクリック購読を登録
                if (_detailPresenter != null && view != null)
                    _detailPresenter.RegisterAlly(view, reference.Ally);
            }
        }
        
        private async UniTaskVoid HandleDeathAsync()
        {
            if (animController != null)
            {
                await animController.PlayDeathAsync();
            }

            _controller.OnDead();
            Destroy(gameObject);
        }

        private void Update()
        {
            _controller?.Tick(Time.deltaTime);
        }

        private void UpdateRegenEffect(float regenPercent)
        {
            bool hasRegen = regenPercent > 0f;
            if (hasRegen)
            {
                if (_regenEffectObj == null && !_isSpawningRegen)
                {
                    _isSpawningRegen = true;
                    SpawnRegenVfxAsync().Forget();
                }
            }
            else
            {
                if (_regenEffectObj != null)
                {
                    Destroy(_regenEffectObj);
                    _regenEffectObj = null;
                }
            }
        }

        private async UniTaskVoid SpawnRegenVfxAsync()
        {
            try
            {
                var go = await UI.EffectVisualFactory.CreateRegenEffectAsync(transform);
                var reference = GetComponent<AllyReference>();
                bool hasRegen = reference != null && reference.Ally != null && reference.Ally.Stats.SelfRegenPercent > 0f;
                if (hasRegen && this != null && gameObject != null)
                {
                    _regenEffectObj = go;
                }
                else
                {
                    Destroy(go);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AllyBattlePresenter] Regen VFX error: {ex.Message}");
            }
            finally
            {
                _isSpawningRegen = false;
            }
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
            _controller?.Dispose();
            _domainPresenter?.Dispose();
        }

        private void OnDrawGizmos()
        {
            if (allyData == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, allyData.SearchRadius);

            Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
            foreach (var grid in allyData.AttackRangeGrids)
            {
                Vector3 localPos = new Vector3(grid.x, 0.5f, grid.y);
                Vector3 worldPos = transform.TransformPoint(localPos);
                
                Gizmos.DrawWireCube(worldPos, Vector3.one * 0.9f);
            }

#if UNITY_EDITOR
            DrawDebugLabels();
#endif
        }

#if UNITY_EDITOR
        private void DrawDebugLabels()
        {
            var reference = GetComponent<AllyReference>();
            if (reference == null || reference.Ally == null) return;

            var ally = reference.Ally;
            var pos = transform.position + Vector3.up * 2.5f;

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 12;

            // 背景用のボックス
            Handles.BeginGUI();
            var screenPos = HandleUtility.WorldToGUIPoint(pos);
            var rect = new Rect(screenPos.x - 60, screenPos.y - 40, 120, 60);
            EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.5f));
            Handles.EndGUI();

            string debugInfo = $"{ally.Stats.Name}\n" +
                               $"HP: {ally.CurrentHp.CurrentValue:F1} / {ally.Stats.MaxHp:F1}\n" +
                               $"Lv: {ally.FusionState.Level}";
            
            Handles.Label(pos, debugInfo, style);
        }
#endif
    }
}

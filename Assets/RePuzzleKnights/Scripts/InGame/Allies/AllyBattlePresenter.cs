using System;
using Cysharp.Threading.Tasks;
using R3;
using RePuzzleKnights.Scripts.InGame.Allies.SO;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Allies
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

        private AllyBattleController controller;
        private AllyBattleModel model;
        private CompositeDisposable disposables = new();

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize(Action<AllyDataSO> onDeath)
        {
            // モデルとコントローラーを生成
            model = new AllyBattleModel(allyData);
            controller = new AllyBattleController(model, allyData, transform);
            
            controller.Initialize(onDeath);
            
            // ビューの初期化
            view.InitializeStatusDisplay(model, allyData.MaxHp);

            // エンティティホルダーにコントローラーを登録
            if (entityHolder == null)
                entityHolder = GetComponent<AllyEntityHolder>();
            
            if (entityHolder != null)
                entityHolder.Initialize(controller);
            
            if (animController == null)
                animController = GetComponentInChildren<AllyAnimationController>();

            // アニメーション速度を攻撃間隔に合わせて設定
            if (animController != null)
            {
                animController.SetAttackSpeed(allyData.AttackInterval);
            }

            // イベント購読
            SubscribeEvents();
        }

        /// <summary>
        /// イベント購読の設定
        /// </summary>
        private void SubscribeEvents()
        {
            // 攻撃リクエスト時にViewを更新
            controller.GetAttackRequestObservable()
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
                            animController.PlayAttack();
                        }
                    }
                })
                .AddTo(disposables);

            // 攻撃キャンセル時にアニメーションをリセット
            controller.GetAttackCancelObservable()
                .Subscribe(_ =>
                {
                    if (animController != null)
                    {
                        animController.StopAttack();
                    }
                })
                .AddTo(disposables);

            // 死亡時の処理
            controller.SubscribeDeathEvent(() =>
            {
                HandleDeathAsync().Forget();
            });
        }
        
        private async UniTaskVoid HandleDeathAsync()
        {
            if (animController != null)
            {
                await animController.PlayDeathAsync();
            }

            controller.OnDead();
            Destroy(gameObject);
        }

        private void Update()
        {
            controller?.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            disposables?.Dispose();
            controller?.Dispose();
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
        }
    }
}
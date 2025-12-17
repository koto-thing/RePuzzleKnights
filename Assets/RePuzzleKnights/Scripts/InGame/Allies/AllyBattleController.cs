using System;
using R3;
using RePuzzleKnights.Scripts.InGame.Allies.Enum;
using RePuzzleKnights.Scripts.InGame.Allies.SO;
using RePuzzleKnights.Scripts.InGame.Enemies.Interface;
using UnityEngine;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.InGame.Allies
{
    public class AllyBattleController : IInitializable, ITickable, IDisposable
    {
        private readonly AllyBattleModel model;
        private readonly AllyView view;
        private readonly AllyDataSO allyData;
        private readonly SphereCollider attackRangeCollider;
        private readonly MonoBehaviour context;

        private CompositeDisposable disposables = new();

        public AllyBattleController(AllyBattleModel model, AllyView view, AllyDataSO data, SphereCollider rangeCollider, MonoBehaviour context)
        {
            this.model = model;
            this.view = view;
            this.allyData = data;
            this.attackRangeCollider = rangeCollider;
            this.context = context;
        }

        public void Initialize()
        {
            if (attackRangeCollider != null)
            {
                attackRangeCollider.radius = allyData.AttackRange;
                attackRangeCollider.isTrigger = true;
            }
            
            SubscribeEvents();
        }

        public void Tick()
        {
            model.UpdateAttackTimer(Time.deltaTime);
            model.CleanUpLists();

            if (allyData.AllyType == AllyType.Ground)
            {
                HandleBlocking();
            }

            if (model.IsAttackReady())
            {
                TryAttack();
            }
        }

        private void SubscribeEvents()
        {
            model.OnAttackRequested
                .Subscribe(target =>
                {
                    if (target != null)
                    {
                        target.TakeDamage(allyData.AttackPower);
                        view.LookAtSnap(target.Position);
                    }
                })
                .AddTo(disposables);
        }

        public void OnEnemyEntered(IEnemyEntity enemy)
        {
            model.AddEnemyInSight(enemy);
        }

        public void OnEnemyExited(IEnemyEntity enemy)
        {
            model.RemoveEnemyInSight(enemy);
            
            if (model.BlockedEnemies.CurrentValue.Contains(enemy))
            {
                enemy.OnReleased();
                model.UnblockEnemy(enemy);
            }
        }

        private void HandleBlocking()
        {
            if (!model.CanBlock())
                return;

            foreach (var enemy in model.EnemiesInSight.CurrentValue)
            {
                if (!model.CanBlock())
                    break;

                if (model.BlockedEnemies.CurrentValue.Contains(enemy))
                    continue;

                if (enemy.IsFlying)
                    continue;

                float distance = Vector3.Distance(context.transform.position, enemy.Position);
                if (distance < 0.5f)
                {
                    Debug.Log($"[AllyBattleController] Blocking enemy at distance {distance}");
                    enemy.OnBlocked(context);
                    model.BlockEnemy(enemy);
                }
            }
        }

        private void TryAttack()
        {
            var target = model.GetBestTarget();
            if (target != null)
            {
                Debug.Log($"[AllyBattleController] Attacking target at distance {Vector3.Distance(context.transform.position, target.Position)}");
                model.RequestAttack(target);
                model.ResetAttackTimer();
            }
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}


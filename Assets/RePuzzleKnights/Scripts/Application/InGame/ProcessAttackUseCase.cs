using System.Collections.Generic;
using R3;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Domain.Enums;
using RePuzzleKnights.Scripts.Domain.Services;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Application.InGame
{
    public class ProcessAttackUseCase
    {
        private readonly Ally _ally;
        private readonly BattleService _battleService;
        private readonly List<IEnemy> _enemiesInSight = new();
        
        public Observable<IEnemy> OnAttackExecuted => _onAttackExecuted;
        private readonly Subject<IEnemy> _onAttackExecuted = new();

        public ProcessAttackUseCase(Ally ally, BattleService battleService)
        {
            _ally = ally;
            _battleService = battleService;
        }

        /// <summary>
        /// 攻撃対象を追加
        /// </summary>
        /// <param name="enemy">目標</param>
        public void AddEnemyInSight(IEnemy enemy)
        {
            if (enemy != null && !_enemiesInSight.Contains(enemy))
                _enemiesInSight.Add(enemy);
        }

        /// <summary>
        /// 攻撃対象から目標を外す
        /// </summary>
        /// <param name="enemy"></param>
        public void RemoveEnemyInSight(IEnemy enemy)
        {
            if (_enemiesInSight.Contains(enemy))
                _enemiesInSight.Remove(enemy);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="myPosition"></param>
        public void Tick(float deltaTime, Vector3 myPosition)
        {
            if (_ally.IsDead.CurrentValue) return;

            _ally.UpdateTimer(deltaTime);

            if (_ally.CanAttack())
            {
                CleanUpLists();

                var target = _ally.GetBestTarget(myPosition, _enemiesInSight);
                if (target != null)
                {
                    PerformAttack(target);
                }
            }
        }

        /// <summary>
        /// 攻撃処理
        /// </summary>
        /// <param name="target">目標</param>
        private void PerformAttack(IEnemy target)
        {
            _battleService.ExecuteAttack(_ally, target, _enemiesInSight);

            _ally.ResetAttackTimer();
            _onAttackExecuted.OnNext(target);
        }

        /// <summary>
        /// 攻撃範囲内の敵をクリーンアップ
        /// </summary>
        private void CleanUpLists()
        {
            _enemiesInSight.RemoveAll(e => e == null || e.IsDead.CurrentValue);
            _ally.CleanUpBlockedEnemies();
        }

        public void Dispose()
        {
            _onAttackExecuted.Dispose();
        }
    }
}



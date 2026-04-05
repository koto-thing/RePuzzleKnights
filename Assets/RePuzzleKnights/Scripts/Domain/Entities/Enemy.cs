using System.Collections.Generic;
using R3;
using RePuzzleKnights.Scripts.Domain.Enums;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Domain.Entities
{
    public class Enemy : IEnemy
    {
        public string Id { get; }
        public EnemyStats Stats { get; }
        public Vector3 CurrentPosition { get; private set; }
        
        public ReadOnlyReactiveProperty<float> CurrentHp => _currentHp;
        private readonly ReactiveProperty<float> _currentHp;
        
        public ReadOnlyReactiveProperty<bool> IsDead => _isDead;
        private readonly ReactiveProperty<bool> _isDead = new(false);
        
        private List<Vector3> _wayPoints;
        private int _currentWaypointIndex;
        
        public ReadOnlyReactiveProperty<Vector3> CurrentTarget => _currentTarget;
        private readonly ReactiveProperty<Vector3> _currentTarget = new();

        public Observable<Unit> OnGoalReached => _onGoalReached;
        private readonly Subject<Unit> _onGoalReached = new();

        public bool IsMoving { get; private set; } = true;
        
        public ReadOnlyReactiveProperty<Ally> CurrentBlocker => _currentBlocker;
        private readonly ReactiveProperty<Ally> _currentBlocker = new();
        private List<Ally> _blockedBy = new();

        public Enemy(string id, EnemyStats stats, List<Vector3> path, Vector3 initialPosition)
        {
            Id = id;
            Stats = stats;
            _wayPoints = path ?? new List<Vector3>();
            CurrentPosition = initialPosition;
            
            _currentHp = new ReactiveProperty<float>(stats.MaxHp);
            _currentWaypointIndex = 0;
        }
        
        /// <summary>
        /// 移動を開始する
        /// </summary>
        public void StartMoving()
        {
            if (_wayPoints.Count > 0)
            {
                _currentTarget.Value = _wayPoints[0];
            }
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="damage">受けるダメージ</param>
        public void TakeDamage(float damage)
        {
            if (_isDead.Value) 
                return;

            _currentHp.Value -= damage;
            if (_currentHp.Value <= 0)
            {
                _currentHp.Value = 0;
                _isDead.Value = true;
            }
        }
        
        /// <summary>
        /// 位置を更新
        /// </summary>
        /// <param name="position">移動先の位置</param>
        public void UpdatePosition(Vector3 position)
        {
            CurrentPosition = position;
        }

        /// <summary>
        /// 目標地点に到達したときの処理
        /// </summary>
        public void OnArrivedAtTarget()
        {
            if (!IsMoving || _isDead.Value) 
                return;

            while (true)
            {
                _currentWaypointIndex++;
                if (_currentWaypointIndex >= _wayPoints.Count)
                {
                    _onGoalReached.OnNext(Unit.Default);
                    return;
                }

                Vector3 nextPoint = _wayPoints[_currentWaypointIndex];
                float dist = Vector3.Distance(_currentTarget.Value, nextPoint);
                if (dist > 0.01f)
                {
                    _currentTarget.Value = nextPoint;
                    return;
                }
            }
        }
        
        public Vector3 Position => CurrentPosition;

        public bool IsFlying => Stats.MoveType == MovementType.FLYING;

        public ElementType Element => Stats.Element;
        
        /// <summary>
        /// ブロックしている味方をセット
        /// </summary>
        /// <param name="blocker">味方</param>
        public void SetBlocker(Ally blocker)
        {
            if (IsFlying && Stats.IgnoreTerrain) 
                return;
            
            if (!_blockedBy.Contains(blocker))
            {
                _blockedBy.Add(blocker);
                if (_currentBlocker.Value == null)
                {
                    _currentBlocker.Value = blocker;
                    IsMoving = false;
                }
            }
        }
        
        /// <summary>
        /// ブロックしている味方を除去
        /// </summary>
        /// <param name="blocker">ブロックしている味方</param>
        public void RemoveBlocker(Ally blocker)
        {
            if (_blockedBy.Contains(blocker))
            {
                _blockedBy.Remove(blocker);
                if (_currentBlocker.Value == blocker)
                {
                    if (_blockedBy.Count > 0)
                    {
                        _currentBlocker.Value = _blockedBy[0];
                    }
                    else
                    {
                        _currentBlocker.Value = null;
                        IsMoving = true;
                    }
                }
            }
        }
        
        /// <summary>
        /// ブロックしたときの処理
        /// </summary>
        /// <param name="ally">味方</param>
        public void OnBlocked(Ally ally) => SetBlocker(ally);
        
        /// <summary>
        /// ブロック状態から解放された時の処理
        /// </summary>
        public void OnReleased() 
        {
            if (_currentBlocker.Value != null)
            {
                RemoveBlocker(_currentBlocker.Value);
            }
        }
    }
}



using System.Collections.Generic;
using UnityEngine;
using R3;
using RePuzzleKnights.Scripts.InGame.Enemies.SO;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    /// <summary>
    /// 敵の移動ロジックを管理するクラス
    /// 経路に沿った移動、ブロック処理、ゴール到達判定を担当
    /// </summary>
    public class EnemyMover
    {
        private List<Vector3> wayPoints;
        private int currentIndex;
        private EnemyDataSO data;
        
        // 現在の目標地点
        public ReadOnlyReactiveProperty<Vector3> CurrentTarget => currentTarget;
        private readonly ReactiveProperty<Vector3> currentTarget = new();

        // ゴール到達時の通知
        public Observable<Unit> OnGoalReached => onGoalReached;
        private readonly Subject<Unit> onGoalReached = new();

        // 移動中かどうか
        public bool IsMoving { get; set; } = true;

        /// <summary>
        /// 移動システムの初期化
        /// </summary>
        public void Initialize(EnemyDataSO data, List<Vector3> path)
        {
            this.data = data;
            this.wayPoints = path;
            this.currentIndex = 0;
            IsMoving = true;

            int count = wayPoints != null ? wayPoints.Count : 0;
            if (wayPoints != null && count > 0)
            {
                currentTarget.Value = wayPoints[0];
            }
        }
        
        /// <summary>
        /// 目標地点に到着した際の処理
        /// 次の地点を設定するか、ゴール到達を通知する
        /// </summary>
        public void OnArrivedAtTarget()
        {
            if (!IsMoving) 
                return;

            // 次の有効な地点が見つかるまでループする
            while (true)
            {
                currentIndex++;

                // 経路の最後まで来た場合 -> ゴール
                if (currentIndex >= wayPoints.Count)
                {
                    onGoalReached.OnNext(Unit.Default);
                    return;
                }

                Vector3 nextPoint = wayPoints[currentIndex];
                float dist = Vector3.Distance(currentTarget.Value, nextPoint);

                // 現在のターゲットと十分離れている場合のみ更新して通知する
                if (dist > 0.01f)
                {
                    currentTarget.Value = nextPoint;
                    return;
                }
            }
        }
        
        /// <summary>
        /// ブロック状態を設定
        /// </summary>
        public void SetBlocked(bool isBlocked)
        {
            if (data.MoveType == MovementType.FLYING && data.IgnoreTerrain) return;
            
            IsMoving = !isBlocked;
        }
    }
}
using System.Collections.Generic;
using R3;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    /// <summary>
    /// 敵の状態（ステルス、スタン、アンストッパブル等）を管理するクラス
    /// </summary>
    public class EnemyStateController
    {
        // 現在の状態セット
        private readonly HashSet<EnemyState> currentStates = new();
        
        // 状態の変化を通知するSubject
        public Observable<EnemyState> OnStateAdded => onStateAdded;
        private readonly Subject<EnemyState> onStateAdded = new();

        /// <summary>
        /// 初期状態で初期化
        /// </summary>
        public void Initialize(List<EnemyState> initialStates)
        {
            currentStates.Clear();
            if (initialStates != null)
            {
                foreach (var state in initialStates) 
                    AddState(state);
            }
        }

        /// <summary>
        /// 状態を追加
        /// </summary>
        public void AddState(EnemyState state)
        {
            if (currentStates.Add(state)) 
                onStateAdded.OnNext(state);
        }

        /// <summary>
        /// 状態を削除
        /// </summary>
        public void RemoveState(EnemyState state)
        {
            currentStates.Remove(state);
        }

        /// <summary>
        /// 指定した状態を持っているか判定
        /// </summary>
        public bool HasState(EnemyState state) => currentStates.Contains(state);
    }
}
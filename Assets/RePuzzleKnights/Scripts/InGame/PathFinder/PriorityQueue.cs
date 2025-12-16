using System;
using System.Collections.Generic;
using R3.Collections;

namespace RePuzzleKnights.Scripts.InGame.PathFinder
{
    public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
    {
        // 最小ヒープを使用した優先度付きキュー
        private readonly List<(TElement Element, TPriority Priority)> heap;

        // キュー内の要素数
        public int Count => heap.Count;
        
        public PriorityQueue()
        {
            heap = new List<(TElement Element, TPriority Priority)>();
        }

        /// <summary>
        /// 要素をキューに追加
        /// </summary>
        /// <param name="element">追加する要素</param>
        /// <param name="priority">優先度</param>
        public void Enqueue(TElement element, TPriority priority)
        {
            heap.Add((element, priority));
            SiftUp(Count - 1);
        }

        /// <summary>
        /// 優先度の最も高い要素をキューから削除して返す
        /// </summary>
        /// <returns>優先度の最も高い要素</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public TElement Dequeue()
        {
            // キューが空の場合は例外をスロー
            if (Count == 0)
                throw new InvalidOperationException("The priority queue is empty.");

            // ルート要素を取得
            var result = heap[0].Element;

            // 最後の要素をルートに移動し、ヒープを再構築
            heap[0] = heap[Count - 1];
            heap.RemoveAt(Count - 1);
            
            if (Count > 0)
                SiftDown(0);

            return result;
        }

        /// <summary>
        /// 要素を上方向に移動してヒープ条件を維持
        /// </summary>
        /// <param name="index">操作する要素</param>
        private void SiftUp(int index)
        {
            // ルートに到達した場合は終了
            if (index == 0)
                return;

            // 親要素と比較して必要に応じて交換
            var parentIndex = (index - 1) / 2;
            if (heap[index].Priority.CompareTo(heap[parentIndex].Priority) < 0)
            {
                (heap[index], heap[parentIndex]) = (heap[parentIndex], heap[index]);
                SiftUp(parentIndex);
            }
        }

        /// <summary>
        /// 要素を下方向に移動してヒープ条件を維持
        /// </summary>
        /// <param name="index">操作する要素</param>
        private void SiftDown(int index)
        {
            // 子要素と比較して必要に応じて交換
            var leftChildIndex = 2 * index + 1;
            var rightChildIndex = 2 * index + 2;
            var smallestChildIndex = index;
            
            // 左の子要素と比較
            if (leftChildIndex < Count && heap[leftChildIndex].Priority.CompareTo(heap[smallestChildIndex].Priority) < 0)
                smallestChildIndex = leftChildIndex;
            
            // 右の子要素と比較
            if (rightChildIndex < Count && heap[rightChildIndex].Priority.CompareTo(heap[smallestChildIndex].Priority) < 0)
                smallestChildIndex = rightChildIndex;

            // 交換が必要な場合は交換して再帰的にSiftDownを呼び出す
            if (smallestChildIndex != index)
            {
                (heap[index], heap[smallestChildIndex]) = (heap[smallestChildIndex], heap[index]);
                SiftDown(smallestChildIndex);
            }
        }
    }
}
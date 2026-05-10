using System.Collections.Generic;
using System.Linq;
using RePuzzleKnights.Scripts.Domain.Enums;

namespace RePuzzleKnights.Scripts.Domain.Entities
{
    /// <summary>
    /// 味方の融合状態を管理するエンティティ
    /// </summary>
    public class FusionState
    {
        public int Level { get; private set; } = 1;
        public bool IsEvolved { get; private set; } = false;
        
        private readonly List<ElementType> _elementHistory = new();
        public IReadOnlyList<ElementType> ElementHistory => _elementHistory;

        public float AttackBonus { get; private set; } = 1.0f;
        public float HpBonus { get; private set; } = 1.0f;

        public FusionState(ElementType initialElement)
        {
            _elementHistory.Add(initialElement);
            Level = 1;
            IsEvolved = false;
        }

        /// <summary>
        /// 融合による強化
        /// </summary>
        public void AddElement(ElementType element)
        {
            // 同じ属性を重ねた場合にボーナス（例：火属性同士なら攻撃力アップ）
            if (_elementHistory.Last() == element)
            {
                if (element == ElementType.Fire) AttackBonus += 0.1f;
                if (element == ElementType.Water) HpBonus += 0.1f;
                // 他の属性のボーナスもここに追加可能
            }

            _elementHistory.Add(element);
            if (Level < 3)
            {
                Level++;
            }
        }

        /// <summary>
        /// 最終進化フラグを立てる
        /// </summary>
        public void Evolve(ElementType element)
        {
            _elementHistory.Add(element);
            IsEvolved = true;
        }

        /// <summary>
        /// 履歴の中で最も多い属性を取得する
        /// </summary>
        public ElementType GetDominantElement()
        {
            if (_elementHistory.Count == 0) return ElementType.Normal;

            return _elementHistory
                .GroupBy(e => e)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key) // 同数の場合はEnum順（必要に応じて優先順位を定義可能）
                .First()
                .Key;
        }
    }
}

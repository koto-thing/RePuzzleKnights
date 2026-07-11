using System.Collections.Generic;
using R3;
using RePuzzleKnights.Scripts.Domain.Enums;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Domain.Entities
{
    /// <summary>
    /// 各属性ごとの所持Soulコストを管理するドメインエンティティ。
    /// </summary>
    public class SoulWallet
    {
        private readonly Dictionary<ElementType, ReactiveProperty<int>> _souls = new();

        public SoulWallet()
        {
            // 初期状態は各属性0
            _souls[ElementType.Fire] = new ReactiveProperty<int>(0);
            _souls[ElementType.Water] = new ReactiveProperty<int>(0);
            _souls[ElementType.Grass] = new ReactiveProperty<int>(0);
            _souls[ElementType.Light] = new ReactiveProperty<int>(0);
            _souls[ElementType.Dark] = new ReactiveProperty<int>(0);
        }

        /// <summary>
        /// 指定した属性の所持数をReactivePropertyとして取得する
        /// </summary>
        public ReadOnlyReactiveProperty<int> GetSoulCount(ElementType element)
        {
            if (_souls.TryGetValue(element, out var prop))
            {
                return prop;
            }
            return null;
        }

        /// <summary>
        /// Soulコストを増やす
        /// </summary>
        public void AddSoul(ElementType element, int amount)
        {
            if (amount <= 0) return;
            if (_souls.TryGetValue(element, out var prop))
            {
                prop.Value += amount;
            }
        }

        /// <summary>
        /// Soulコストを消費する。消費に成功したら true を返す。
        /// </summary>
        public bool ConsumeSoul(ElementType element, int amount)
        {
            if (amount < 0) return false;
            if (_souls.TryGetValue(element, out var prop))
            {
                if (prop.Value >= amount)
                {
                    prop.Value -= amount;
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Soulコストが消費可能か確認する
        /// </summary>
        public bool CanConsumeSoul(ElementType element, int amount)
        {
            if (_souls.TryGetValue(element, out var prop))
            {
                return prop.Value >= amount;
            }
            return false;
        }
    }
}

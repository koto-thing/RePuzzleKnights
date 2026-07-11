using R3;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Domain.Enums;

namespace RePuzzleKnights.Scripts.Application.InGame
{
    /// <summary>
    /// 各属性ごとのSoulの増減および消費を管理するユースケース。
    /// </summary>
    public class SoulUseCase
    {
        private readonly SoulWallet _soulWallet;

        public SoulUseCase(SoulWallet soulWallet)
        {
            _soulWallet = soulWallet;
        }

        /// <summary>
        /// 指定した属性の現在の所持数を監視するReactivePropertyを取得する
        /// </summary>
        public ReadOnlyReactiveProperty<int> GetSoulCount(ElementType element)
        {
            return _soulWallet.GetSoulCount(element);
        }

        /// <summary>
        /// 指定した属性のSoulコストを追加する
        /// </summary>
        public void AddSoul(ElementType element, int amount)
        {
            _soulWallet.AddSoul(element, amount);
        }

        /// <summary>
        /// 指定した属性のSoulコストを消費する。消費できた場合は true を返す。
        /// </summary>
        public bool ConsumeSoul(ElementType element, int amount)
        {
            return _soulWallet.ConsumeSoul(element, amount);
        }

        /// <summary>
        /// 指定した属性のSoulコストを消費できるか判定する
        /// </summary>
        public bool CanConsumeSoul(ElementType element, int amount)
        {
            return _soulWallet.CanConsumeSoul(element, amount);
        }
    }
}

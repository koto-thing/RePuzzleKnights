using System;
using R3;
using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Enums;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    /// <summary>
    /// 各属性ごとのSoulコストの表示制御を仲介するPresenter。
    /// </summary>
    public class SoulPresenter : IStartable, IDisposable
    {
        private readonly SoulUseCase _soulUseCase;
        private readonly ISoulCostView _view;
        private readonly CompositeDisposable _disposables = new();

        public SoulPresenter(SoulUseCase soulUseCase, ISoulCostView view)
        {
            _soulUseCase = soulUseCase;
            _view = view;
        }

        public void Start()
        {
            // 全属性の変更をそれぞれ購読してViewに反映させる
            foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
            {
                if (element == ElementType.Normal) continue;
                
                var el = element; // クロージャキャプチャ用
                var soulCountProp = _soulUseCase.GetSoulCount(el);
                if (soulCountProp != null)
                {
                    soulCountProp
                        .Subscribe(soul =>
                        {
                            _view.SetSoulCost(el, soul);
                        })
                        .AddTo(_disposables);
                }
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}

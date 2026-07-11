using System;
using System.Collections.Generic;
using R3;
using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Domain.Enums;
using VContainer.Unity;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    /// <summary>
    /// キャラクタークリック時に詳細パネルを開閉する Presenter。
    /// Application 層（FusionUseCase）と View 層（IAllyDetailPanelView）のみに依存する。
    /// </summary>
    public class AllyDetailPresenter : IStartable, IDisposable
    {
        private readonly IAllyDetailPanelView _panelView;
        private readonly FusionUseCase _fusionUseCase;
        private readonly CompositeDisposable _disposables = new();

        private Ally _currentAlly;

        // 属性ごとのアビリティ説明テキスト
        private static readonly Dictionary<ElementType, string> AbilityDescs = new()
        {
            { ElementType.Fire,   "【炎上】攻撃時に確定で敵を炎上させ、継続ダメージを与える。Lvアップで効果増大。" },
            { ElementType.Water,  "【減速】攻撃時に敵の移動速度を低下させる。対空攻撃優先。" },
            { ElementType.Grass,  "【回復】一定時間ごとに自身のHPを自動回復。Lvアップでブロック数増加。" },
            { ElementType.Light,  "【気絶】攻撃時に低確率で敵をスタン。Lvアップで攻撃対象数増加。" },
            { ElementType.Dark,   "【回避＆防御デバフ】物理攻撃を確率で回避し、敵の防御力を低下させる。" },
            { ElementType.Normal, "通常攻撃を行う。" }
        };

        public AllyDetailPresenter(IAllyDetailPanelView panelView, FusionUseCase fusionUseCase)
        {
            _panelView = panelView;
            _fusionUseCase = fusionUseCase;
        }

        /// <summary>VContainer が呼ぶ Start で Backdrop コールバックをバインド</summary>
        public void Start()
        {
            _panelView.SetBackdropCallback(() =>
            {
                _currentAlly = null;
                _panelView.Hide();
            });
        }

        /// <summary>味方ユニット生成時に呼ぶ。クリック購読を登録する。</summary>
        public void RegisterAlly(IAllyView view, Ally ally)
        {
            view.OnClicked
                .Subscribe(_ => OnAllyClicked(ally))
                .AddTo(_disposables);
        }

        private void OnAllyClicked(Ally ally)
        {
            // 同じキャラを再クリック → 閉じる
            if (_currentAlly != null && _currentAlly.Id == ally.Id)
            {
                _currentAlly = null;
                _panelView.Hide();
                return;
            }

            _currentAlly = ally;
            _panelView.Show(BuildViewData(ally));
        }

        private AllyDetailViewData BuildViewData(Ally ally)
        {
            var s = ally.Stats;
            var data = new AllyDetailViewData
            {
                Name = s.Name,
                Level = ally.FusionState.Level,
                IsEvolved = ally.FusionState.IsEvolved,
                Element = s.Element,
                IsHighGround = s.PlacementType == Domain.Enums.PlacementType.HighGround,
                Hp = s.MaxHp,
                Atk = s.AttackPower,
                Block = s.BlockCount,
                AttackInterval = s.AttackInterval,
                SplashRadius = s.SplashRadius,
                CanAttackFlying = s.CanAttackFlying,
                AbilityDesc = AbilityDescs.TryGetValue(s.Element, out var desc) ? desc : "",
                ElementHistory = new List<ElementType>(ally.FusionState.ElementHistory)
            };

            // 次回強化プレビュー（進化済みでなければ計算）
            if (!ally.FusionState.IsEvolved)
            {
                var preview = _fusionUseCase.PreviewFusion(ally, s);
                data.HpDiff = preview.HpDiff;
                data.AtkDiff = preview.AtkDiff;
                data.NextJobName = preview.NextJobName;
                data.IsNextEvolution = preview.IsEvolution;
            }

            return data;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using R3;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Domain.Enums;
using RePuzzleKnights.Scripts.Domain.Repositories;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Application.InGame
{
    /// <summary>
    /// 融合ロジックを管理するユースケース
    /// </summary>
    public class FusionUseCase
    {
        private readonly IAllyDataRepository _allyDataRepository;
        
        // 属性とジョブの対応表
        private readonly Dictionary<ElementType, string> _elementToJobMap = new()
        {
            { ElementType.Fire, "Sword" },
            { ElementType.Water, "Archer" },
            { ElementType.Grass, "Shield" },
            { ElementType.Light, "Mage" },
            { ElementType.Dark, "Assassin" }
        };

        public Observable<(Ally baseAlly, Ally addedAlly)> OnFusionPerformed => _onFusionPerformed;
        private readonly Subject<(Ally, Ally)> _onFusionPerformed = new();

        public Observable<(Ally ally, string newJob)> OnEvolutionPerformed => _onEvolutionPerformed;
        private readonly Subject<(Ally, string)> _onEvolutionPerformed = new();

        public FusionUseCase(IAllyDataRepository allyDataRepository)
        {
            this._allyDataRepository = allyDataRepository;
        }

        /// <summary>
        /// 融合可能かどうかを判定
        /// </summary>
        public bool CanFuse(Ally baseAlly, Ally addedAlly)
        {
            if (baseAlly == null || addedAlly == null) return false;
            if (baseAlly.FusionState.IsEvolved) return false; // 進化済みは融合不可

            // レベル3にレベル1を重ねると進化が発生する（仕様調整）
            if (baseAlly.FusionState.Level == 3 && addedAlly.FusionState.Level == 1)
            {
                return true;
            }

            // レベル3未満なら、ベースにレベル1を重ねて強化可能
            if (baseAlly.FusionState.Level < 3 && addedAlly.FusionState.Level == 1)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 融合を実行する
        /// </summary>
        public void PerformFusion(Ally baseAlly, Ally addedAlly)
        {
            Debug.Log($"PerformFusion: BaseLv={baseAlly.FusionState.Level}, AddedLv={addedAlly.FusionState.Level}");
            
            if (!CanFuse(baseAlly, addedAlly)) 
            {
                Debug.LogWarning("PerformFusion: Fusion not possible.");
                return;
            }

            if (baseAlly.FusionState.Level == 3)
            {
                // 進化（Lv3 + Lv1 で発生）
                baseAlly.FusionState.Evolve(addedAlly.Stats.Element);
                
                // 最多属性に基づいてジョブを決定
                ElementType dominant = baseAlly.FusionState.GetDominantElement();
                string jobName = _elementToJobMap.ContainsKey(dominant) ? _elementToJobMap[dominant] : "Sword";
                
                Debug.Log($"<color=cyan>Evolution Success!</color> Dominant: {dominant}, Job: {jobName}");
                
                _onEvolutionPerformed.OnNext((baseAlly, jobName));
            }
            else
            {
                // 強化 (Lv1 -> Lv2, Lv2 -> Lv3)
                baseAlly.FusionState.AddElement(addedAlly.Stats.Element);
                
                // ステータスボーナスの適用
                var oldStats = baseAlly.Stats;
                var newStats = new AllyStats(
                    oldStats.Name,
                    oldStats.PlacementType,
                    oldStats.Type,
                    oldStats.Element,
                    oldStats.MaxHp * baseAlly.FusionState.HpBonus,
                    oldStats.AttackPower * baseAlly.FusionState.AttackBonus,
                    oldStats.AttackRange,
                    oldStats.AttackInterval,
                    oldStats.BlockCount,
                    oldStats.SearchRadius,
                    oldStats.RangeType,
                    oldStats.Priority,
                    oldStats.SplashRadius,
                    oldStats.CanAttackFlying
                );
                
                baseAlly.UpdateStats(newStats);
                
                Debug.Log($"<color=green>Strengthen Success!</color> Level: {baseAlly.FusionState.Level}, Atk: {newStats.AttackPower}");
                _onFusionPerformed.OnNext((baseAlly, addedAlly));
            }
        }
    }
}

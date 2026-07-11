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
                
                // マスの配置タイプに応じた最終進化の強制補正 (アークナイツ配置制限ルール)
                PlacementType currentPlacement = baseAlly.Stats.PlacementType;
                var groundElements = new HashSet<ElementType> { ElementType.Fire, ElementType.Grass, ElementType.Dark };
                var highGroundElements = new HashSet<ElementType> { ElementType.Water, ElementType.Light };

                if (currentPlacement == PlacementType.HighGround && groundElements.Contains(dominant))
                {
                    // 高台なのに地上専用属性になってしまった場合、履歴から高台専用属性(Water/Light)のうち数の多い方へ補正
                    var history = baseAlly.FusionState.ElementHistory;
                    int waterCount = history.Count(e => e == ElementType.Water);
                    int lightCount = history.Count(e => e == ElementType.Light);
                    dominant = (waterCount == 0 && lightCount == 0) ? ElementType.Water : (waterCount >= lightCount ? ElementType.Water : ElementType.Light);
                    Debug.Log($"[Evolution] Enforced HighGround constraint. Shifted dominant job element from Melee to Ranged: {dominant}");
                }
                else if (currentPlacement == PlacementType.Ground && highGroundElements.Contains(dominant))
                {
                    // 地上なのに高台専用属性になってしまった場合、履歴から地上専用属性(Fire/Grass/Dark)のうち数の多い方へ補正
                    var history = baseAlly.FusionState.ElementHistory;
                    var counts = new Dictionary<ElementType, int>
                    {
                        { ElementType.Fire, history.Count(e => e == ElementType.Fire) },
                        { ElementType.Grass, history.Count(e => e == ElementType.Grass) },
                        { ElementType.Dark, history.Count(e => e == ElementType.Dark) }
                    };
                    int maxVal = counts.Values.Max();
                    dominant = maxVal == 0 ? ElementType.Fire : counts.FirstOrDefault(kv => kv.Value == maxVal).Key;
                    Debug.Log($"[Evolution] Enforced Ground constraint. Shifted dominant job element from Ranged to Melee: {dominant}");
                }

                string jobName = _elementToJobMap.ContainsKey(dominant) ? _elementToJobMap[dominant] : "Sword";
                
                Debug.Log($"<color=cyan>Evolution Success!</color> Dominant: {dominant}, Job: {jobName}");
                
                _onEvolutionPerformed.OnNext((baseAlly, jobName));
            }
            else
            {
                // 強化 (Lv1 -> Lv2, Lv2 -> Lv3)
                baseAlly.FusionState.AddElement(addedAlly.Stats.Element);
                int nextLevel = baseAlly.FusionState.Level;
                
                var oldStats = baseAlly.Stats;
                
                // レベルアップに伴うアビリティ・ステータスの再計算
                StatusEffectType customEffectType = oldStats.CustomEffectType;
                float customEffectDuration = oldStats.CustomEffectDuration;
                float customEffectValue = oldStats.CustomEffectValue;
                float customEffectProbability = oldStats.CustomEffectProbability;
                float selfRegenPercent = oldStats.SelfRegenPercent;
                float reflectDamagePercent = oldStats.ReflectDamagePercent;
                float dodgeChance = oldStats.DodgeChance;
                int maxTargets = oldStats.MaxTargets;
                bool isSplash = oldStats.IsSplash;
                int blockCount = oldStats.BlockCount;
                
                var newGrids = new List<GridCoordinate>(oldStats.AttackRangeGrids);

                switch (oldStats.Element)
                {
                    case ElementType.Fire:
                        if (nextLevel == 2)
                        {
                            customEffectValue = 20f;
                            newGrids = new List<GridCoordinate> { new(0, 1), new(0, 2) };
                        }
                        else if (nextLevel == 3)
                        {
                            customEffectValue = 35f;
                            newGrids = new List<GridCoordinate> { new(0, 1), new(0, 2), new(-1, 1), new(1, 1) };
                        }
                        break;
                    case ElementType.Water:
                        if (nextLevel == 2)
                        {
                            customEffectValue = 0.45f;
                            newGrids = new List<GridCoordinate> { new(0, 1), new(0, 2), new(0, 3) };
                        }
                        else if (nextLevel == 3)
                        {
                            customEffectValue = 0.60f;
                            newGrids = new List<GridCoordinate> { new(0, 1), new(0, 2), new(0, 3), new(0, 4), new(-1, 2), new(1, 2) };
                        }
                        break;
                    case ElementType.Grass:
                        if (nextLevel == 2)
                        {
                            blockCount = 2;
                            selfRegenPercent = 0.02f;
                            reflectDamagePercent = 0.15f;
                            newGrids = new List<GridCoordinate> { new(0, 0), new(0, 1) };
                        }
                        else if (nextLevel == 3)
                        {
                            blockCount = 3;
                            selfRegenPercent = 0.035f;
                            reflectDamagePercent = 0.25f;
                            newGrids = new List<GridCoordinate> { new(0, 0), new(0, 1), new(-1, 0), new(1, 0) };
                        }
                        break;
                    case ElementType.Light:
                        if (nextLevel == 2)
                        {
                            customEffectProbability = 0.15f;
                            maxTargets = 2;
                            newGrids = new List<GridCoordinate> { new(0, 1), new(0, 2), new(0, 3), new(-1, 2), new(1, 2) };
                        }
                        else if (nextLevel == 3)
                        {
                            customEffectProbability = 0.25f;
                            isSplash = true;
                            newGrids = new List<GridCoordinate> { new(0, 1), new(0, 2), new(0, 3), new(0, 4), new(-1, 2), new(1, 2), new(-1, 3), new(1, 3) };
                        }
                        break;
                    case ElementType.Dark:
                        if (nextLevel == 2)
                        {
                            dodgeChance = 0.3f;
                            customEffectValue = 20f;
                            customEffectProbability = 0.7f;
                            newGrids = new List<GridCoordinate> { new(0, 1), new(0, 2) };
                        }
                        else if (nextLevel == 3)
                        {
                            dodgeChance = 0.4f;
                            customEffectValue = 35f;
                            customEffectProbability = 1.0f;
                            newGrids = new List<GridCoordinate> { new(0, 1), new(0, 2), new(-1, 1), new(1, 1) };
                        }
                        break;
                }

                var newStats = new AllyStats(
                    oldStats.Name,
                    oldStats.PlacementType,
                    oldStats.Type,
                    oldStats.Element,
                    oldStats.MaxHp * baseAlly.FusionState.HpBonus,
                    oldStats.AttackPower * baseAlly.FusionState.AttackBonus,
                    oldStats.AttackRange,
                    oldStats.AttackInterval,
                    blockCount,
                    oldStats.SearchRadius,
                    oldStats.RangeType,
                    oldStats.Priority,
                    oldStats.SplashRadius,
                    oldStats.CanAttackFlying,
                    newGrids,
                    customEffectType,
                    customEffectDuration,
                    customEffectValue,
                    customEffectProbability,
                    selfRegenPercent,
                    reflectDamagePercent,
                    dodgeChance,
                    maxTargets,
                    isSplash
                );
                
                baseAlly.UpdateStats(newStats);
                
                Debug.Log($"<color=green>Strengthen Success!</color> Level: {baseAlly.FusionState.Level}, Atk: {newStats.AttackPower}");
                _onFusionPerformed.OnNext((baseAlly, addedAlly));
            }
        }

        /// <summary>
        /// 融合（強化・進化）後のステータスと攻撃範囲のプレビューを計算して返す
        /// </summary>
        public FusionPreviewInfo PreviewFusion(Ally baseAlly, AllyStats draggingStats)
        {
            var info = new FusionPreviewInfo();
            if (baseAlly == null || draggingStats == null) return info;

            if (baseAlly.FusionState.Level == 3)
            {
                // 進化プレビュー (Lv3 ➔ 最終進化)
                info.IsEvolution = true;

                // 属性履歴の仮シミュレーション
                var tempHistory = new List<ElementType>(baseAlly.FusionState.ElementHistory) { draggingStats.Element };
                
                ElementType dominant = tempHistory
                    .GroupBy(e => e)
                    .OrderByDescending(g => g.Count())
                    .ThenBy(g => g.Key)
                    .First()
                    .Key;

                // 配置制限に基づく dominant 属性の補正
                PlacementType currentPlacement = baseAlly.Stats.PlacementType;
                var groundElements = new HashSet<ElementType> { ElementType.Fire, ElementType.Grass, ElementType.Dark };
                var highGroundElements = new HashSet<ElementType> { ElementType.Water, ElementType.Light };

                if (currentPlacement == PlacementType.HighGround && groundElements.Contains(dominant))
                {
                    int waterCount = tempHistory.Count(e => e == ElementType.Water);
                    int lightCount = tempHistory.Count(e => e == ElementType.Light);
                    dominant = (waterCount == 0 && lightCount == 0) ? ElementType.Water : (waterCount >= lightCount ? ElementType.Water : ElementType.Light);
                }
                else if (currentPlacement == PlacementType.Ground && highGroundElements.Contains(dominant))
                {
                    var counts = new Dictionary<ElementType, int>
                    {
                        { ElementType.Fire, tempHistory.Count(e => e == ElementType.Fire) },
                        { ElementType.Grass, tempHistory.Count(e => e == ElementType.Grass) },
                        { ElementType.Dark, tempHistory.Count(e => e == ElementType.Dark) }
                    };
                    int maxVal = counts.Values.Max();
                    dominant = maxVal == 0 ? ElementType.Fire : counts.FirstOrDefault(kv => kv.Value == maxVal).Key;
                }

                string jobName = _elementToJobMap.ContainsKey(dominant) ? _elementToJobMap[dominant] : "Sword";
                info.NextJobName = jobName;

                // 進化後のデータを取得 (インターフェース経由でクリーンに)
                var evolutionData = _allyDataRepository.GetAllyDataByJobName(jobName);
                if (evolutionData != null)
                {
                    info.HpDiff = evolutionData.MaxHp - baseAlly.Stats.MaxHp;
                    info.AtkDiff = evolutionData.AttackPower - baseAlly.Stats.AttackPower;
                    
                    info.NextAttackRangeGrids = new List<GridCoordinate>();
                    if (evolutionData.AttackRangeGrids != null)
                    {
                        foreach (var grid in evolutionData.AttackRangeGrids)
                        {
                            info.NextAttackRangeGrids.Add(new GridCoordinate(grid.x, grid.y));
                        }
                    }
                }
            }
            else
            {
                // 強化プレビュー (Lv1 ➔ Lv2, Lv2 ➔ Lv3)
                info.IsEvolution = false;

                int nextLevel = baseAlly.FusionState.Level + 1;
                float hpBonus = 1.0f;
                float atkBonus = 1.0f;

                if (baseAlly.FusionState.ElementHistory.Last() == draggingStats.Element)
                {
                    if (draggingStats.Element == ElementType.Fire) atkBonus += 0.1f;
                    if (draggingStats.Element == ElementType.Water) hpBonus += 0.1f;
                }

                float baseMaxHp = baseAlly.Stats.MaxHp / baseAlly.FusionState.HpBonus;
                float baseAtk = baseAlly.Stats.AttackPower / baseAlly.FusionState.AttackBonus;
                
                float nextHpBonus = baseAlly.FusionState.HpBonus + (hpBonus - 1.0f);
                float nextAtkBonus = baseAlly.FusionState.AttackBonus + (atkBonus - 1.0f);

                info.HpDiff = (baseMaxHp * nextHpBonus) - baseAlly.Stats.MaxHp;
                info.AtkDiff = (baseAtk * nextAtkBonus) - baseAlly.Stats.AttackPower;

                var newGrids = new List<GridCoordinate>();
                switch (baseAlly.Stats.Element)
                {
                    case ElementType.Fire:
                        if (nextLevel == 2) newGrids = new List<GridCoordinate> { new(0, 1), new(0, 2) };
                        else if (nextLevel == 3) newGrids = new List<GridCoordinate> { new(0, 1), new(0, 2), new(-1, 1), new(1, 1) };
                        break;
                    case ElementType.Water:
                        if (nextLevel == 2) newGrids = new List<GridCoordinate> { new(0, 1), new(0, 2), new(0, 3) };
                        else if (nextLevel == 3) newGrids = new List<GridCoordinate> { new(0, 1), new(0, 2), new(0, 3), new(0, 4), new(-1, 2), new(1, 2) };
                        break;
                    case ElementType.Grass:
                        if (nextLevel == 2) newGrids = new List<GridCoordinate> { new(0, 0), new(0, 1) };
                        else if (nextLevel == 3) newGrids = new List<GridCoordinate> { new(0, 0), new(0, 1), new(-1, 0), new(1, 0) };
                        break;
                    case ElementType.Light:
                        if (nextLevel == 2) newGrids = new List<GridCoordinate> { new(0, 1), new(0, 2), new(0, 3), new(-1, 2), new(1, 2) };
                        else if (nextLevel == 3) newGrids = new List<GridCoordinate> { new(0, 1), new(0, 2), new(0, 3), new(0, 4), new(-1, 2), new(1, 2), new(-1, 3), new(1, 3) };
                        break;
                    case ElementType.Dark:
                        if (nextLevel == 2) newGrids = new List<GridCoordinate> { new(0, 1), new(0, 2) };
                        else if (nextLevel == 3) newGrids = new List<GridCoordinate> { new(0, 1), new(0, 2), new(-1, 1), new(1, 1) };
                        break;
                }
                info.NextAttackRangeGrids = newGrids;
            }

            return info;
        }
    }

    /// <summary>
    /// 融合プレビュー情報を格納するDTO
    /// </summary>
    public class FusionPreviewInfo
    {
        public List<GridCoordinate> NextAttackRangeGrids = new();
        public float HpDiff = 0f;
        public float AtkDiff = 0f;
        public string NextJobName = string.Empty;
        public bool IsEvolution = false;
    }
}

using System.Collections.Generic;
using RePuzzleKnights.Scripts.Domain.Enums;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    /// <summary>
    /// AllyDetailPanelView に渡す表示用 DTO。
    /// Presentation 層で組み立て、View 層に渡す。
    /// </summary>
    public class AllyDetailViewData
    {
        public string Name;
        public int Level;
        public bool IsEvolved;
        public ElementType Element;
        public bool IsHighGround;

        public float Hp;
        public float Atk;
        public int Block;
        public float AttackInterval;   // 秒
        public float SplashRadius;
        public bool CanAttackFlying;

        public float HpDiff;           // 次回強化後との差分
        public float AtkDiff;

        public string NextJobName;     // 進化先ジョブ名（Lv3→進化時のみ）
        public bool IsNextEvolution;

        public string AbilityDesc;
        public List<ElementType> ElementHistory = new();
    }

    /// <summary>
    /// キャラクター詳細パネル（画面左）のViewインターフェース。
    /// Presentation 層のみから参照される。
    /// </summary>
    public interface IAllyDetailPanelView
    {
        void Show(AllyDetailViewData data);
        void Hide();

        /// <summary>パネル外（Backdrop）クリック時に呼ぶコールバックをPresenterから設定する</summary>
        void SetBackdropCallback(System.Action onBackdropClicked);
    }
}

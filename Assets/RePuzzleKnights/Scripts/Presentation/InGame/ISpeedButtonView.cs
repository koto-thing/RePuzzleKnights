using R3;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    /// <summary>
    /// 倍速切り替えボタンのUIアクションと表示を制御するインターフェース。
    /// </summary>
    public interface ISpeedButtonView
    {
        /// <summary>
        /// ボタンがクリックされたときのイベント
        /// </summary>
        Observable<Unit> OnClick { get; }

        /// <summary>
        /// 現在の速度表示を更新する
        /// </summary>
        void UpdateSpeedVisual(float speed);
    }
}

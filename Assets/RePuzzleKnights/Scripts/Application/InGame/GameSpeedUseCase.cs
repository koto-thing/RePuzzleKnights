using R3;

namespace RePuzzleKnights.Scripts.Application.InGame
{
    /// <summary>
    /// ゲームの進行速度（Time.timeScale）を切り替えるユースケース。
    /// </summary>
    public class GameSpeedUseCase
    {
        private readonly ReactiveProperty<float> _currentSpeed = new(1.0f);
        public ReadOnlyReactiveProperty<float> CurrentSpeed => _currentSpeed;

        /// <summary>
        /// 速度をトグルで切り替える (1.0倍速 <-> 2.0倍速)
        /// </summary>
        public void ToggleSpeed()
        {
            if (_currentSpeed.Value == 1.0f)
            {
                _currentSpeed.Value = 2.0f;
            }
            else
            {
                _currentSpeed.Value = 1.0f;
            }
        }
    }
}

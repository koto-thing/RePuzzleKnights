using R3;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    public interface IAllyView
    {
        Vector3 Position { get; }

        /// <summary>クリック時に発火するObservable（Presenterが購読する）</summary>
        Observable<Unit> OnClicked { get; }
        
        void PlayAttackAnimation();
        void PlayDamageAnimation();
        void PlayDieAnimation();
        void UpdateHpBar(float current, float max);
    }
}



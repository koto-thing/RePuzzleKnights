using UnityEngine;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    public interface IAllyView
    {
        Vector3 Position { get; }
        
        void PlayAttackAnimation();
        void PlayDamageAnimation();
        void PlayDieAnimation();
        void UpdateHpBar(float current, float max);
    }
}



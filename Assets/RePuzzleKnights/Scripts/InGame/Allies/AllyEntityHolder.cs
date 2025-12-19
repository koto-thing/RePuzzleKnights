using RePuzzleKnights.Scripts.InGame.Allies.Interface;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Allies
{
    public class AllyEntityHolder : MonoBehaviour
    {
        public IAllyEntity Entity { get; private set; }

        public void Initialize(IAllyEntity entity)
        {
            Entity = entity;
        }
    }
}
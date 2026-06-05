using RePuzzleKnights.Scripts.Domain.Entities;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.Allies
{
    /// <summary>
    /// GameObjectからDomainのAllyエンティティへの参照を保持するコンポーネント
    /// </summary>
    public class AllyReference : MonoBehaviour
    {
        public Ally Ally { get; private set; }

        public void Initialize(Ally ally)
        {
            Ally = ally;
        }
    }
}

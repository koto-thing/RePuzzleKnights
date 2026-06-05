using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Entities;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    public class AllyController
    {
        private readonly ProcessAttackUseCase _useCase;
        private readonly Ally _ally;

        public AllyController(ProcessAttackUseCase useCase, Ally ally)
        {
            this._useCase = useCase;
            this._ally = ally;
        }

        public void Tick(float deltaTime, Vector3 position)
        {
            _useCase.Tick(deltaTime, position);
        }

        public void OnEnemyEnterRange(IEnemy enemy)
        {
            _useCase.AddEnemyInSight(enemy);
        }

        public void OnEnemyExitRange(IEnemy enemy)
        {
            _useCase.RemoveEnemyInSight(enemy);
        }
    }
}



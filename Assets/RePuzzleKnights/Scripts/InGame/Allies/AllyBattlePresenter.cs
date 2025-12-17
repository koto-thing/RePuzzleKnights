using RePuzzleKnights.Scripts.InGame.Allies.SO;
using RePuzzleKnights.Scripts.InGame.Enemies.Interface;
using UnityEngine;

namespace RePuzzleKnights.Scripts.InGame.Allies
{
    public class AllyBattlePresenter : MonoBehaviour
    {
        [SerializeField] private AllyView view;
        [SerializeField] private SphereCollider attackRangeCollider;
        [SerializeField] private AllyDataSO allyData;

        private AllyBattleController controller;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            var model = new AllyBattleModel(allyData);
            controller = new AllyBattleController(model, view, allyData, attackRangeCollider, this);
            controller.Initialize();
        }

        private void Update()
        {
            controller?.Tick();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IEnemyEntity>(out var enemy))
            {
                Debug.Log($"[AllyBattlePresenter] Enemy entered range: {other.gameObject.name}");
                controller?.OnEnemyEntered(enemy);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<IEnemyEntity>(out var enemy))
            {
                Debug.Log($"[AllyBattlePresenter] Enemy exited range: {other.gameObject.name}");
                controller?.OnEnemyExited(enemy);
            }
        }

        private void OnDestroy()
        {
            controller?.Dispose();
        }
    }
}
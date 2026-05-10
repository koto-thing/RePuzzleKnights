using System;
using Cysharp.Threading.Tasks;
using R3;
using RePuzzleKnights.Scripts.Application.InGame;
using RePuzzleKnights.Scripts.Domain.Entities;
using RePuzzleKnights.Scripts.Infrastructure.InGame;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Presentation.InGame
{
    public class AllyPresenter : IDisposable
    {
        private readonly Ally _ally;
        private readonly FusionUseCase _fusionUseCase;
        private readonly IAllyView _view;
        private readonly AllyFactory _allyFactory;
        private readonly AddressableAllyDataRepository _allyDataRepository;
        private readonly GameObject _gameObject;
        private readonly CompositeDisposable _disposables = new();

        public AllyPresenter(
            Ally ally, 
            FusionUseCase fusionUseCase, 
            IAllyView view,
            AllyFactory allyFactory,
            AddressableAllyDataRepository allyDataRepository,
            GameObject gameObject)
        {
            this._ally = ally;
            this._fusionUseCase = fusionUseCase;
            this._view = view;
            this._allyFactory = allyFactory;
            this._allyDataRepository = allyDataRepository;
            this._gameObject = gameObject;
        }

        public void Initialize()
        {
            _ally.CurrentHp
                .Subscribe(hp => _view.UpdateHpBar(hp, _ally.Stats.MaxHp))
                .AddTo(_disposables);

            _ally.IsDead
                .Where(isDead => isDead)
                .Subscribe(_ => _view.PlayDieAnimation())
                .AddTo(_disposables);

            // 融合イベントの監視
            _fusionUseCase.OnFusionPerformed
                .Where(x => x.baseAlly.Id == _ally.Id)
                .Subscribe(_ => HandleFusion())
                .AddTo(_disposables);

            _fusionUseCase.OnEvolutionPerformed
                .Where(x => x.ally.Id == _ally.Id)
                .Subscribe(x => HandleEvolutionAsync(x.newJob).Forget())
                .AddTo(_disposables);
        }

        private void HandleFusion()
        {
            Debug.Log($"Ally {_ally.Id} fused. New Level: {_ally.FusionState.Level}");
        }

        private async UniTaskVoid HandleEvolutionAsync(string jobName)
        {
            Debug.Log($"<color=cyan>AllyPresenter: Evolution Start for {jobName}</color>");
            
            // 進化後のデータを取得
            var evolutionData = _allyDataRepository.GetAllyDataByJobName(jobName);
            if (evolutionData == null)
            {
                Debug.LogError($"Evolution data for {jobName} not found!");
                return;
            }

            // 現在の位置と回転を保持
            Vector3 currentPos = _gameObject.transform.position;
            Quaternion currentRot = _gameObject.transform.rotation;

            // 新しいユニットを生成
            await _allyFactory.CreateAllyAsync(evolutionData, currentPos, currentRot);

            // 古い自分自身を破棄
            Debug.Log($"Replacing old unit with {jobName}");
            UnityEngine.Object.Destroy(_gameObject);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}

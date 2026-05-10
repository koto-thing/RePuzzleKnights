using System.Collections.Generic;
using System.Linq;
using RePuzzleKnights.Scripts.Domain.Repositories;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO;
using UnityEngine;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.Allies
{
    public class AddressableAllyDataRepository : IAllyDataRepository
    {
        private readonly List<AllyDataSO> _allAllyData = new();

        public AddressableAllyDataRepository(List<AllyDataSO> allAllyData)
        {
            _allAllyData = allAllyData;
        }

        public AllyDataSO GetAllyDataByName(string name)
        {
            return _allAllyData.FirstOrDefault(d => d.AllyName == name);
        }

        public AllyDataSO GetAllyDataByJobName(string jobName)
        {
            // 名前の中にジョブ名（Sword, Archerなど）が含まれているものを探す
            return _allAllyData.FirstOrDefault(d => d.AllyName.Contains(jobName));
        }

        public IEnumerable<AllyDataSO> GetAllAllyData()
        {
            return _allAllyData;
        }
    }
}

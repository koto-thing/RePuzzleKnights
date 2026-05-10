using System.Collections.Generic;
using RePuzzleKnights.Scripts.Infrastructure.InGame.Allies.SO;

namespace RePuzzleKnights.Scripts.Domain.Repositories
{
    public interface IAllyDataRepository
    {
        AllyDataSO GetAllyDataByName(string name);
        IEnumerable<AllyDataSO> GetAllAllyData();
    }
}

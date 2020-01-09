using System.Collections.Generic;
using System.Threading.Tasks;
using Barforce_Backend.Model.Drink.Overview;
using Barforce_Backend.Model.User;

namespace Barforce_Backend.Interface.Repositories
{
    public interface IHistoryRepository
    {
        Task<List<OverviewDrink>> ReadUsersHistory(int userId, int take, int skip);
        Task<double> GetUsersAlcoholStatus(int userId);
    }
}
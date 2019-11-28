using System.Collections.Generic;
using System.Threading.Tasks;
using Barforce_Backend.Model.Drink;
using Barforce_Backend.Model.Drink.Favourite;
using Barforce_Backend.Model.Drink.Overview;

namespace Barforce_Backend.Interface.Repositories
{
    public interface IDrinkRepository
    {
        Task<List<OverviewDrink>> ReadUsersHistory(int userId, int take, int skip);
        Task<IEnumerable<GlassSize>> ReadGlassSizes();
        Task<int> CreateOrder(int userId, int machineId, CreateDrink newDrink);
        Task<int> AddFavourite(int userId, NewFavouriteDrink newNewFavourite);
        Task<List<FavouriteDrink>> GetFavouriteDrinks(int userId);
        Task RemoveFavouriteDrink(int userId, int drinkId);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Barforce_Backend.Model.Drink;
using Barforce_Backend.Model.Drink.Favorite;

namespace Barforce_Backend.Interface.Repositories
{
    public interface IDrinkRepository
    {
        Task<IEnumerable<GlassSize>> ReadGlassSizes();
        Task<int> CreateOrder(int userId, int machineId, CreateDrink newDrink);
        Task<int> AddFavorite(int userId, NewFavoriteDrink newNewFavorite);
        Task<List<FavoriteDrink>> GetFavoriteDrinks(int userId);
        Task DeleteFavoriteDrink(int userId, int drinkId);
    }
}
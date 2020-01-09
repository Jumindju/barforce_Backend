using System.Collections.Generic;
using System.Threading.Tasks;
using Barforce_Backend.Model.Drink.Favourite;

namespace Barforce_Backend.Interface.Repositories
{
    public interface IFavoriteRepository
    {
        Task<int> AddFavourite(int userId, NewFavouriteDrink newNewFavourite);
        Task<List<FavouriteDrink>> GetFavouriteDrinks(int userId);
        Task RemoveFavouriteDrink(int userId, int drinkId);
    }
}
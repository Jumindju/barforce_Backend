using System.Collections.Generic;
using System.Threading.Tasks;
using Barforce_Backend.Model.Drink;
using Barforce_Backend.Model.Drink.Favourite;
using Barforce_Backend.Model.Drink.Overview;
using Barforce_Backend.Model.Ingredient;
using Barforce_Backend.Model.Websocket;

namespace Barforce_Backend.Interface.Repositories
{
    public interface IDrinkRepository
    {
        Task<IEnumerable<GlassSize>> ReadGlassSizes();
        Task<int> CreateOrder(int userId, int machineId, CreateDrink newDrink);
        Task<(int drinkId, int glassSize)> GetDrink(CreateDrink newDrink);
        Task<List<DrinkIngredient>> GetIngredientsOfDrink(int drinkId);
    }
}
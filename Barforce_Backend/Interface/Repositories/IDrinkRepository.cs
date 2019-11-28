﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Barforce_Backend.Model.Drink;
using Barforce_Backend.Model.Drink.Favourite;

namespace Barforce_Backend.Interface.Repositories
{
    public interface IDrinkRepository
    {
        Task<IEnumerable<GlassSize>> ReadGlassSizes();
        Task<int> CreateOrder(int userId, int machineId, CreateDrink newDrink);
        Task<int> AddFavourite(int userId, NewFavouriteDrink newNewFavourite);
        Task<List<FavouriteDrink>> GetFavouriteDrinks(int userId);
        Task DeleteFavouriteDrink(int userId, int drinkId);
    }
}
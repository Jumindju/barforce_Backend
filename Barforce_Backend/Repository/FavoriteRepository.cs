using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Drink.Favourite;
using Barforce_Backend.Model.Helper.Middleware;
using Dapper;

namespace Barforce_Backend.Repository
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly IDbHelper _dbHelper;
        private readonly IDrinkRepository _drinkRepository;

        public FavoriteRepository(IDbHelper dbHelper, IDrinkRepository drinkRepository)
        {
            _dbHelper = dbHelper;
            _drinkRepository = drinkRepository;
        }
        public async Task<List<FavouriteDrink>> GetFavouriteDrinks(int userId)
        {
            const string getDrinksCmd = @"SELECT 
                                                userid,
                                                drinkid,
                                                glasssize,
                                                glasssizeid,
                                                ""name""
                                            from vifavouritedrink
                                            where userid=:userid";
            var getDrinksParameter = new DynamicParameters(new
            {
                userId
            });
            List<FavouriteDrink> favouriteDrinks;
            try
            {
                using var con = await _dbHelper.GetConnection();
                var favDrinksRaw = await con.QueryAsync<FavouriteDrink>(getDrinksCmd, getDrinksParameter);
                favouriteDrinks = favDrinksRaw.ToList();
            }
            catch (Exception e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError,
                    "Error while getting favourite drinks", e);
            }

            foreach (var favouriteDrink in favouriteDrinks)
            {
                favouriteDrink.Ingredients = await _drinkRepository.GetIngredientsOfDrink(favouriteDrink.DrinkId);
            }

            return favouriteDrinks;
        }
        public async Task<int> AddFavourite(int userId, NewFavouriteDrink newNewFavourite)
        {
            if (string.IsNullOrEmpty(newNewFavourite?.Name))
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "No favourite name");
            var (drinkId, _) = await _drinkRepository.GetDrink(newNewFavourite);
            if (await FavoriteDrinkExists(userId, drinkId))
                throw new HttpStatusCodeException(HttpStatusCode.NotModified, "Drink is already users favorite");
            const string cmd = @"INSERT INTO favouritedrink
                                (
                                    userid, drinkid, ""name""                                
                                )
                                VALUES(
                                       :userId,
                                       :drinkId,
                                       :drinkName
                                )";
            var parameter = new DynamicParameters(new
            {
                userId,
                drinkId,
                drinkName = newNewFavourite.Name
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                await con.ExecuteAsync(cmd, parameter);
            }
            catch (Exception e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, "Error while adding favourite",
                    e);
            }

            return drinkId;
        }

        public async Task RemoveFavouriteDrink(int userId, int drinkId)
        {
            if (!await FavoriteDrinkExists(userId, drinkId))
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "Favorite drink doesnt exists");
            const string cmd =
                "UPDATE favouriteDrink SET deletetime=current_timestamp WHERE drinkid=:drinkId AND userid=:userId";
            var parameter = new DynamicParameters(new
            {
                drinkId,
                userId
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                await con.ExecuteAsync(cmd, parameter);
            }
            catch (Exception e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError,
                    "Error while removing favorite drink", e);
            }
        }

        private async Task<bool> FavoriteDrinkExists(int userId, int drinkId)
        {
            const string cmd = @"SELECT drinkId FROM vifavouritedrink WHERE drinkId=:drinkId AND userId=:userId";
            var parameter = new DynamicParameters(new
            {
                userId,
                drinkId
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                var drinkExists = await con.ExecuteScalarAsync<int?>(cmd, parameter);
                return drinkExists != null;
            }
            catch (Exception e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError,
                    "Error while checking if favorite drink exists", e);
            }
        }
    }
}
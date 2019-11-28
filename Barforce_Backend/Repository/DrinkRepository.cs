﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Drink;
using Barforce_Backend.Model.Drink.Favourite;
using Barforce_Backend.Model.Helper.Middleware;
using Barforce_Backend.Model.Ingredient;
using Dapper;

namespace Barforce_Backend.Repository
{
    public class DrinkRepository : IDrinkRepository
    {
        private readonly IDbHelper _dbHelper;
        private readonly IContainerRepo _containerRepo;

        public DrinkRepository(IDbHelper dbHelper, IContainerRepo containerRepo)
        {
            _dbHelper = dbHelper;
            _containerRepo = containerRepo;
        }

        public async Task<IEnumerable<GlassSize>> ReadGlassSizes()
        {
            const string cmd = @"SELECT
                    id,
                    size
                FROM glasssize";
            using var con = await _dbHelper.GetConnection();
            return await con.QueryAsync<GlassSize>(cmd);
        }

        public async Task<int> CreateOrder(int userId, int machineId, CreateDrink newDrink)
        {
            var drinkId = await GetDrink(newDrink, machineId);
            const string createOrderCmd = @"INSERT INTO ""order""
                                            (
                                             userid,
                                             drinkId
                                            );
                                            VALUES(
                                                :userId,
                                                 :drinkId 
                                            )
                                            RETURNING id";
            var createOrderParams = new DynamicParameters(new
            {
                userId,
                drinkId
            });
            int orderId;
            try
            {
                using var con = await _dbHelper.GetConnection();
                orderId = await con.ExecuteScalarAsync<int>(createOrderCmd, createOrderParams);
            }
            catch (Exception e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, "Could not create drink", e);
            }

            //TODO: Call Arduino
            await Task.Delay(3000);
            var rnd = new Random();
            var drinksInQueue = rnd.Next(0, 7);

            const string setServeTimeCmd = @"
                UPDATE ""order"" 
                SET servetime=:serveTime 
                WHERE id=:id";
            var serveParameter = new DynamicParameters(new
            {
                id = orderId,
                serveTime = DateTime.UtcNow
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                await con.ExecuteAsync(setServeTimeCmd, serveParameter);
            }
            catch (Exception e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, "Could not set serveTime", e);
            }

            return drinksInQueue;
        }

        public async Task<int> AddFavourite(int userId, NewFavouriteDrink newNewFavourite)
        {
            if (string.IsNullOrEmpty(newNewFavourite?.Name))
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "No favourite name");
            var drinkId = await GetDrink(newNewFavourite);
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

            const string favDrinkIngredientsCmd = @"SELECT 
                                                           id   as ingredientId,
                                                           amount,
                                                           name as ingredientName,
                                                           alcohollevel,
                                                           background
                                                    FROM drink2liquid
                                                             join viingredient on ingredientid = id
                                                    WHERE drinkid = :drinkid";
            foreach (var favouriteDrink in favouriteDrinks)
            {
                var drinkParams = new DynamicParameters(new
                {
                    favouriteDrink.DrinkId
                });
                try
                {
                    using var con = await _dbHelper.GetConnection();
                    var ingredientsOfDrink = await con.QueryAsync<DrinkIngredient>(favDrinkIngredientsCmd, drinkParams);
                    favouriteDrink.Ingredients = ingredientsOfDrink.ToList();
                }
                catch (Exception e)
                {
                    throw new HttpStatusCodeException(HttpStatusCode.InternalServerError,
                        "Error while getting ingredients of favourite drink", e);
                }
            }

            return favouriteDrinks;
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
                var con = await _dbHelper.GetConnection();
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
                var con = await _dbHelper.GetConnection();
                var drinkExists = await con.ExecuteScalarAsync<int?>(cmd, parameter);
                return drinkExists != null;
            }
            catch (Exception e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError,
                    "Error while checking if favorite drink exists", e);
            }
        }

        private async Task<int> GetDrink(CreateDrink newDrink, int? machineId = null)
        {
            // Validate Input
            if (newDrink == null)
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "No drink send");
            if (newDrink.Ingredients == null)
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "No ingredients defined");
            if (!await GlassSizeExists(newDrink.GlassSizeId))
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "Glass size doesnt exits");

            if (machineId != null)
                await CheckContainer(machineId.Value, newDrink);

            if (newDrink.Ingredients.Any(ingredient => ingredient.IngredientId == 0))
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "Invalid ingredient send");
            if (newDrink.Ingredients.Sum(ingredient => ingredient.Amount) > 100)
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest,
                    "Maximum total 100 percent per drink allowed");

            var existingDrinkId = await DrinkAlreadyExists(newDrink);
            return existingDrinkId ?? await CreateDrink(newDrink);
        }

        private async Task CheckContainer(int machineId, CreateDrink newDrink)
        {
            // Check if liquid is in containers
            var currentContainers = await _containerRepo.ReadAll(machineId);
            var drinksInContainers = currentContainers.Select(container => container.IngredientId).ToList();
            if (newDrink.Ingredients.Any(ingredient =>
                !drinksInContainers.Contains(ingredient.IngredientId)
                || ingredient.Amount < 0
            ))
            {
                throw new HttpStatusCodeException(HttpStatusCode.Conflict, "Ingredient of drink not in container");
            }
        }

        private async Task<bool> GlassSizeExists(int glassId)
        {
            const string cmd = @"SELECT Id FROM glasssize WHERE Id=:glassId";
            var parameter = new DynamicParameters(new
            {
                glassId
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                return await con.QueryFirstOrDefaultAsync<int?>(cmd, parameter) != null;
            }
            catch (SqlException e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError,
                    "Error while checking if glass exists", e);
            }
        }

        private async Task<int> CreateDrink(CreateDrink newDrink)
        {
            int drinkId;
            const string insertDrinkCmd = @"INSERT INTO drink
                                            (glasssizeid)
                                            VALUES (:glassSizeId)
                                            RETURNING id";
            var parameter = new DynamicParameters(new
            {
                newDrink.GlassSizeId
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                drinkId = await con.QueryFirstAsync<int>(insertDrinkCmd, parameter);
            }
            catch (SqlException e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, "Error while inserting drink", e);
            }

            const string insertIngredientsCmd = @"INSERT INTO drink2liquid
                                                (ingredientid, drinkid, amount)
                                                VALUES 
                                                (:ingredientId,:drinkId,:amount)";
            foreach (var insertDrinksParams in newDrink.Ingredients.Select(newDrinkIngredient => new DynamicParameters(
                new
                {
                    newDrinkIngredient.IngredientId,
                    drinkId,
                    newDrinkIngredient.Amount
                })))
            {
                using var con = await _dbHelper.GetConnection();
                await con.ExecuteAsync(insertIngredientsCmd, insertDrinksParams);
            }

            return drinkId;
        }

        private async Task<int?> DrinkAlreadyExists(CreateDrink drink)
        {
            var possibleDrinkIds = new List<int>();
            var possibleDrinksCmd = @"SELECT DrinkId
                                     FROM drink2liquid 
                                     WHERE 
                                     ingredientid=:ingredientId AND
                                     amount=:amount";
            for (var i = 0; i < drink.Ingredients.Count; i++)
            {
                if (i == 1)
                    possibleDrinksCmd += " AND DrinkId = ANY(:possibleDrinkIds)";
                var drinkIngredient = drink.Ingredients[i];
                var ingredientCheckParams = new DynamicParameters(new
                {
                    drinkIngredient.IngredientId,
                    drinkIngredient.Amount,
                    possibleDrinkIds
                });
                using var con = await _dbHelper.GetConnection();
                var queriedPossibleDrinks = await con.QueryAsync<int>(possibleDrinksCmd, ingredientCheckParams);
                possibleDrinkIds = queriedPossibleDrinks.ToList();
                if (!possibleDrinkIds.Any())
                    return null;
            }

            const string possibleDrinkCmd = @"SELECT Id
                                                FROM drink 
                                                WHERE 
                                                    glasssizeid=:glassSizeId 
                                                  AND id = :possibleDrinkId
                                                AND 
                                                    (SELECT
                                                        COUNT(ingredientid) 
                                                    FROM drink2liquid
                                                    WHERE drinkid=:possibleDrinkId
                                                    )=:ingredientCount";
            foreach (var drinkCheckParams in possibleDrinkIds.Select(possibleDrinkId => new DynamicParameters(new
            {
                drink.GlassSizeId,
                possibleDrinkId,
                ingredientCount = drink.Ingredients.Count
            })))
            {
                try
                {
                    using var con = await _dbHelper.GetConnection();
                    var drinkId = await con.QueryFirstOrDefaultAsync<int?>(possibleDrinkCmd, drinkCheckParams);
                    if (drinkId != null)
                        return drinkId;
                }
                catch (SqlException e)
                {
                    throw new HttpStatusCodeException(HttpStatusCode.InternalServerError,
                        "Error while checking if drink exists", e);
                }
            }

            return null;
        }
    }
}
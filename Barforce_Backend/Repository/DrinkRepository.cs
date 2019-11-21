﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Drink;
using Barforce_Backend.Model.Helper.Middleware;
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

        public async Task CreateDrink(int machineId, CreateDrink newDrink)
        {
            // Validate Input
            if (newDrink == null)
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "No drink send");
            if (newDrink.Ingredients == null)
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "No ingredients defined");
            if (!await GlassSizeExists(newDrink.GlassSizeId))
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "Glass size doesnt exits");
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

            if (newDrink.Ingredients.Sum(ingredient => ingredient.Amount) > 100)
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest,
                    "Maximum total 100 percent per drink allowed");

            // TODO: Check if drink was already inserted

            var existingDrinkId = await DrinkAlreadyExists(newDrink);
            var drinkId = existingDrinkId ?? await CreateDrink(newDrink);
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

            const string insertIngredientsCmd = @"INSERT INTO public.drink2liquid
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
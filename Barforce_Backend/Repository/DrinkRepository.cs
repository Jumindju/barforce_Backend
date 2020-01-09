using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Drink;
using Barforce_Backend.Model.Drink.Favourite;
using Barforce_Backend.Model.Drink.Overview;
using Barforce_Backend.Model.Helper.Middleware;
using Barforce_Backend.Model.Ingredient;
using Barforce_Backend.Model.User;
using Barforce_Backend.Model.Websocket;
using Barforce_Backend.WebSockets;
using Dapper;

namespace Barforce_Backend.Repository
{
    public class DrinkRepository : IDrinkRepository
    {
        private readonly IDbHelper _dbHelper;
        private readonly IContainerRepo _containerRepo;
        private readonly MachineHandler _machineHandler;
        private readonly IUserRepository _userRepo;

        public DrinkRepository(IDbHelper dbHelper, IContainerRepo containerRepo, MachineHandler machineHandler,
            IUserRepository userRepo)
        {
            _dbHelper = dbHelper;
            _containerRepo = containerRepo;
            _machineHandler = machineHandler;
            _userRepo = userRepo;
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
            var (drinkId, glassSize) = await GetDrink(newDrink);

            var drinkCmd = await CheckContainer(machineId, glassSize, newDrink);
            await _containerRepo.IngredientsInContainer(machineId, glassSize, newDrink.Ingredients);

            const string createOrderCmd = @"INSERT INTO ""order""
                                            (
                                             userid,
                                             drinkId
                                            )
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

            var user = await _userRepo.ReadUserById(userId);
            return await _machineHandler.SendMessageToMachine(machineId, user.Username, orderId, drinkCmd);
        }

        public async Task<List<DrinkIngredient>> GetIngredientsOfDrink(int drinkId)
        {
            const string favDrinkIngredientsCmd = @"SELECT 
                                                           id   as ingredientId,
                                                           amount,
                                                           name as ingredientName,
                                                           alcohollevel,
                                                           background
                                                    FROM drink2liquid
                                                             join viingredient on ingredientid = id
                                                    WHERE drinkid = :drinkid";
            var drinkParams = new DynamicParameters(new
            {
                drinkId
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                var ingredientsOfDrink = await con.QueryAsync<DrinkIngredient>(favDrinkIngredientsCmd, drinkParams);
                return ingredientsOfDrink.ToList();
            }
            catch (Exception e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError,
                    "Error while getting ingredients of favourite drink", e);
            }
        }

        public async Task<(int drinkId, int glassSize)> GetDrink(CreateDrink newDrink)
        {
            // Validate Input
            if (newDrink == null)
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "No drink send");
            if (newDrink.Ingredients == null)
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "No ingredients defined");
            var glassSize = await GlassSizeExists(newDrink.GlassSizeId);
            if (glassSize == null)
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "Glass size doesnt exits");

            if (newDrink.Ingredients.Any(ingredient => ingredient.IngredientId == 0))
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "Invalid ingredient send");
            if (newDrink.Ingredients.Sum(ingredient => ingredient.Amount) > 100)
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest,
                    "Maximum total 100 percent per drink allowed");

            var existingDrinkId = await DrinkAlreadyExists(newDrink);
            return existingDrinkId != null
                ? (existingDrinkId.Value, glassSize.Value)
                : (await CreateDrink(newDrink), glassSize.Value);
        }

        private async Task<List<DrinkCommand>> CheckContainer(int machineId, int glassSize, CreateDrink newDrink)
        {
            // Check if liquid is in containers
            var currentContainersRaw = await _containerRepo.ReadAll(machineId);
            var currentContainers = currentContainersRaw.ToList();
            var drinkCommand = new List<DrinkCommand>();
            foreach (var ingredient in newDrink.Ingredients)
            {
                var containerOfIngredient =
                    currentContainers.FirstOrDefault(container => container.IngredientId == ingredient.IngredientId);
                if (containerOfIngredient == null || ingredient.Amount < 0)
                    throw new HttpStatusCodeException(HttpStatusCode.Conflict, "Ingredient of drink not in container");
                drinkCommand.Add(new DrinkCommand
                {
                    Id = containerOfIngredient.Id,
                    AmmountMl = (int) ((ingredient.Amount * glassSize) / 100)
                });
            }

            return drinkCommand;
        }

        private async Task<int?> GlassSizeExists(int glassId)
        {
            const string cmd = @"SELECT size FROM glasssize WHERE Id=:glassId";
            var parameter = new DynamicParameters(new
            {
                glassId
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                return await con.QueryFirstOrDefaultAsync<int?>(cmd, parameter);
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
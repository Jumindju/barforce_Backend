using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Container;
using Barforce_Backend.Model.Helper.Middleware;
using Barforce_Backend.Model.Ingredient;
using Dapper;

namespace Barforce_Backend.Repository
{
    public class ContainerRepo : IContainerRepo
    {
        private readonly IDbHelper _dbHelper;

        public ContainerRepo(IDbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<IEnumerable<ContainerDto>> ReadAll(int machineId)
        {
            const string cmd = @"
                SELECT 
                        id,
                        machinename,
                        fillingvolume,
                        fillinglevel,
                        ingredientid,
                        ingredientName,
                        alcoholLevel,
                        background
                FROM
                    vicontainer
                WHERE
                    machineId=:machineId";
            var parameter = new DynamicParameters(new
            {
                machineId
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                return await con.QueryAsync<ContainerDto>(cmd, parameter);
            }
            catch (Exception e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, "Error while reading containers",
                    e);
            }
        }

        public async Task<bool> IngredientsInContainer(int machineId, int glassSize,
            List<NewDrinkIngredient> ingredients)
        {
            var cmd = $@"SELECT ingredientid
                                        ,fillinglevel 
                                FROM container
                                WHERE 
                                      machineid=:machineId AND
                                    ingredientid IN ({string.Join(',', ingredients.Select(ing => ing.IngredientId))})";
            var parameter = new DynamicParameters(new
            {
                machineId,
            });
            IEnumerable<(int ingredientId, int fillingLevel)> containerFilling;
            try
            {
                using var con = await _dbHelper.GetConnection();
                containerFilling = await con.QueryAsync<(int, int)>(cmd, parameter);
            }
            catch (SqlException e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, "Error while checking container",
                    e);
            }

            foreach (var (ingredientId, fillingLevel) in containerFilling)
            {
                var ingredient = ingredients.FirstOrDefault(ing => ing.IngredientId == ingredientId);
                if (ingredient == null)
                    throw new HttpStatusCodeException(HttpStatusCode.Conflict, "No container has requested ingredient");

                var requestedAmount = glassSize * (ingredient.Amount / 100);
                if (requestedAmount > fillingLevel)
                    throw new HttpStatusCodeException(HttpStatusCode.Conflict, "Container does not have enough liquid");
            }

            return true;
        }
    }
}
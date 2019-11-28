using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Container;
using Barforce_Backend.Model.Helper.Middleware;
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

        public Task<bool> IngredientInContainer(int machineId, List<int> ingredients)
        {
            throw new NotImplementedException();
        }
    }
}
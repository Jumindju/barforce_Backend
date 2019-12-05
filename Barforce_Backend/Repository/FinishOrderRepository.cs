using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Model.Helper.Middleware;
using Barforce_Backend.Model.Websocket;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Barforce_Backend.Interface.Repositories;

namespace Barforce_Backend.Repository
{
    public class FinishOrderRepository : IFinishOrderRepository
    {
        private readonly IDbHelper _dbHelper;
        public FinishOrderRepository(IDbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }
        public async Task FinishOrder(int orderId, List<DrinkCommand> drinks)
        {
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

            const string updateFillingCmd = @"UPDATE container
                                                SET fillinglevel=fillinglevel-:amount
                                                WHERE id=:id";
            foreach (var parameter in drinks.Select(drink =>
                new DynamicParameters(new
                {
                    Amount = drink.AmmountMl,
                    drink.Id
                })))
            {
                using var con = await _dbHelper.GetConnection();
                await con.ExecuteAsync(updateFillingCmd, parameter);
            }
        }
    }
}

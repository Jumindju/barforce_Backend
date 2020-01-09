using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Drink.Overview;
using Barforce_Backend.Model.Helper.Middleware;
using Dapper;

namespace Barforce_Backend.Repository
{
    public class HistoryRepository : IHistoryRepository
    {
        private readonly IDbHelper _dbHelper;
        private readonly IDrinkRepository _drinkRepository;

        public HistoryRepository(IDbHelper dbHelper, IDrinkRepository drinkRepository)
        {
            _dbHelper = dbHelper;
            _drinkRepository = drinkRepository;
        }
        public async Task<List<OverviewDrink>> ReadUsersHistory(int userId, int take, int skip)
        {
            List<OverviewDrink> drinkHistory;
            const string cmd = @"SELECT drinkid,
                                       orderdate,
                                       servetime,
                                       size as glassSize
                                FROM ""order""
                                            JOIN vidrink d on drinkid = d.id
                                WHERE
                                    userId=:userId AND
                                    serveTime IS NOT NULL
                                ORDER BY
                                    orderdate desc
                                OFFSET :skip LIMIT :take";
            var parameter = new DynamicParameters(new
            {
                userId,
                skip,
                take
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                var drinkHistoryRaw = await con.QueryAsync<OverviewDrink>(cmd, parameter);
                drinkHistory = drinkHistoryRaw.ToList();
            }
            catch (Exception e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError,
                    "Error while reading users order history", e);
            }

            foreach (var drink in drinkHistory)
            {
                drink.Ingredients = await _drinkRepository.GetIngredientsOfDrink(drink.DrinkId);
            }

            return drinkHistory;
        }
        public Task<double> GetUsersAlcoholStatus(int userId)
        {
            throw new System.NotImplementedException();
        }
    }
}
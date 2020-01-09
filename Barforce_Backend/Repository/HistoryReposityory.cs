using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Drink.Overview;
using Barforce_Backend.Model.Helper.Middleware;
using Barforce_Backend.Model.User;
using Dapper;

namespace Barforce_Backend.Repository
{
    public class HistoryRepository : IHistoryRepository
    {
        private readonly IDbHelper _dbHelper;
        private readonly IDrinkRepository _drinkRepository;
        private readonly IUserRepository _userRepository;

        public HistoryRepository(IDbHelper dbHelper, IDrinkRepository drinkRepository, IUserRepository userRepository)
        {
            _dbHelper = dbHelper;
            _drinkRepository = drinkRepository;
            _userRepository = userRepository;
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

        public async Task<double> GetUsersAlcoholStatus(int userId)
        {
            var user = await _userRepository.ReadUserById(userId);
            if (user.Weight == null)
                throw new HttpStatusCodeException(HttpStatusCode.Conflict,
                    "User has to specify weight to get his alcohol status");
            const string cmd = @"WITH DrinkStats AS (
                                    SELECT servetime
                                         , sum((alcohollevel / 100.0) * ((amount / 100.0) * size)) AS AlcoholLevel
                                         , ((SUM(amount) / 100.0) * size)                          AS DrinkSize
                                    FROM ""order"" o
                                             JOIN vidrink d on o.drinkid = d.id
                                             JOIN drink2liquid d2l on d.id = d2l.drinkid
                                             JOIN vicontainer v on d2l.ingredientid = v.ingredientid
                                    WHERE userId = :userId
                                      AND servetime is not null
                                    group by servetime, size, userid
                                )
                                SELECT servetime
                                     , DrinkSize * 0.8 * (AlcoholLevel / CAST(DrinkSize AS FLOAT)) AS AlcAmount
                                FROM DrinkStats
                                ORDER BY servetime ASC";
            var parameter = new DynamicParameters(new
            {
                userId
            });
            List<AlcoholStats> alcStats;
            try
            {
                using var con = await _dbHelper.GetConnection();
                var alcStatsRaw = await con.QueryAsync<AlcoholStats>(cmd, parameter);
                alcStats = alcStatsRaw.ToList();
            }
            catch (Exception e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError,
                    "Unhandled error while getting users alc status", e);
            }

            var currentAlcoholLevel = 0.0;
            var prevDate = DateTime.MinValue;
            var bodyLiquidAmount = user.Gender == Gender.Male ? 0.68 : 0.55;
            foreach (var alcStat in alcStats)
            {
                if (prevDate != DateTime.MinValue)
                {
                    currentAlcoholLevel = GetAlcLevel(currentAlcoholLevel, alcStat.ServeTime, prevDate);
                }

                var bloodAlcConcentration = alcStat.AlcAmount / (user.Weight.Value * bodyLiquidAmount);
                currentAlcoholLevel += bloodAlcConcentration;

                prevDate = alcStat.ServeTime;
            }

            // Substract current level
            var lastServeTime = alcStats.Last().ServeTime;
            return GetAlcLevel(currentAlcoholLevel, DateTime.UtcNow, lastServeTime);
        }

        private static double GetAlcLevel(double currentAlcLevel, DateTime currentDate, DateTime lastDate)
        {
            var hourDifference = (currentDate - lastDate).TotalHours;
            var brookDownAlc = hourDifference * 0.1;
            if (brookDownAlc >= currentAlcLevel)
                currentAlcLevel = 0.0;
            else
                currentAlcLevel -= brookDownAlc;
            return currentAlcLevel;
        }
    }
}
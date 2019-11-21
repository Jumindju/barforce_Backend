using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Drink;
using Dapper;

namespace Barforce_Backend.Repository
{
    public class DrinkRepository : IDrinkRepository
    {
        private readonly IDbHelper _dbHelper;
        public DrinkRepository(IDbHelper dbHelper)
        {
            _dbHelper = dbHelper;
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

        public async Task<int> CreateDrink(CreateDrink newDrink)
        {
            throw new NotImplementedException();
        }
    }
}
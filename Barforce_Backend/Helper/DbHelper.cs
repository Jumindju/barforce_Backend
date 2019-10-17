using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Model.Helper.Database;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Barforce_Backend.Helper
{
    public class DbHelper : IDbHelper
    {
        private readonly DbSettings _dbSettings;
        public DbHelper(IOptions<DbSettings> dbSettings)
        {
            _dbSettings = dbSettings.Value;
        }
        public async Task<IDbConnection> GetConnection(CancellationToken ct = default)
        {
            try
            {
                using var con = new NpgsqlConnection(_dbSettings.ConnectionString);
                await con.OpenAsync(ct);
                return con;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while opening db con: {e.Message}");
                throw;
            }
        }
    }
}

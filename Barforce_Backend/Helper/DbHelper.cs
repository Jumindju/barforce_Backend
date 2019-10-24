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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Barforce_Backend.Helper
{
    public class DbHelper : IDbHelper
    {
        private readonly DbSettings _dbSettings;
        private readonly IWebHostEnvironment _hostEnvironment;
        public DbHelper(IOptions<DbSettings> dbSettings, IWebHostEnvironment hostEnvironment)
        {
            _dbSettings = dbSettings.Value;
            _hostEnvironment = hostEnvironment;
        }
        public async Task<IDbConnection> GetConnection(CancellationToken ct = default)
        {
            try
            {
                var con = new NpgsqlConnection(GetConnectionString());
                await con.OpenAsync(ct);
                return con;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while opening db con: {e.Message}");
                throw;
            }
        }
        private string GetConnectionString()
        {
            return _hostEnvironment.IsDevelopment()
                ? _dbSettings.ConnectionString
                : PostgresUriToConString(Environment.GetEnvironmentVariable("DATABASE_URL"));
        }

        private static string PostgresUriToConString(string postgresUri)
        {
            var remove = postgresUri = postgresUri.Remove(0, postgresUri.IndexOf("://", StringComparison.Ordinal));
            var conStringInformation = postgresUri.Split(new[] {':', '@', '/'}, StringSplitOptions.RemoveEmptyEntries);
            return $"UserName={conStringInformation[0]};Password={conStringInformation[1]};Host={conStringInformation[2]};Database={conStringInformation[4]}";
        }
    }
}

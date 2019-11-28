using Barforce_Backend.Interface.Helper;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Data;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Barforce_Backend.Model.Configuration;
using Barforce_Backend.Model.Helper.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Barforce_Backend.Helper
{
    public class DbHelper : IDbHelper
    {
        private readonly DbSettings _dbSettings;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ILogger _logger;

        public DbHelper(IOptions<DbSettings> dbSettings, IWebHostEnvironment hostEnvironment,
            ILoggerFactory loggerFactory)
        {
            _dbSettings = dbSettings.Value;
            _hostEnvironment = hostEnvironment;
            _logger = loggerFactory.CreateLogger<DbHelper>();
        }

        public async Task<IDbConnection> GetConnection(CancellationToken ct = default)
        {
            var conString = GetConnectionString();
            try
            {
                var con = new NpgsqlConnection(conString);
                await con.OpenAsync(ct);
                return con;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while opening sql connection");
                throw new HttpStatusCodeException(HttpStatusCode.ServiceUnavailable,
                    $"Could not open sql connection", e);
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
            var conStringInformation = postgresUri.Split(new[] {':', '@', '/'}, StringSplitOptions.RemoveEmptyEntries);
            return
                $"UserName={conStringInformation[1]};Password={conStringInformation[2]};Host={conStringInformation[3]};Database={conStringInformation[5]}";
        }
    }
}
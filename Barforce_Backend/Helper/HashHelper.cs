using System;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Model.Helper.Middleware;
using Barforce_Backend.Model.User;
using Dapper;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace Barforce_Backend.Helper
{
    public class HashHelper : IHashHelper
    {
        private readonly IDbHelper _dbHelper;
        private readonly ILogger _logger;
        private const string SaltChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public HashHelper(IDbHelper dbHelper, ILoggerFactory loggerFactory)
        {
            _dbHelper = dbHelper;
            _logger = loggerFactory.CreateLogger<HashHelper>();
        }

        public string GetHash(string clearPw, string salt)
        {
            using var argon2 = new Argon2i(
                Encoding.UTF8.GetBytes(clearPw)
            )
            {
                Salt = Encoding.UTF8.GetBytes(salt),
                DegreeOfParallelism = 4,
                Iterations = 2,
                MemorySize = 128
            };
            var hashedPw = argon2.GetBytes(32);
            return Convert.ToBase64String(hashedPw);
        }

        public bool IsCorrectPassword(string clearPw, string salt, string checkPw)
        {
            var hashedPw = GetHash(clearPw, salt);
            return hashedPw == checkPw;
        }

        public async Task<string> CreateSalt()
        {
            var random = new Random();
            for (var tries = 0; tries < 5; tries++)
            {
                var newSalt = new StringBuilder();
                for (var i = 0; i < 16; i++)
                {
                    var rndIndex = random.Next(0, SaltChars.Length);
                    newSalt.Append(SaltChars[rndIndex]);
                }

                var stringSalt = newSalt.ToString();
                if (await IsSaltUnique(stringSalt))
                    return stringSalt;
            }
            throw new HttpStatusCodeException(HttpStatusCode.Conflict,"Could not get salt after 5 retries");
        }

        private async Task<bool> IsSaltUnique(string salt)
        {
            const string cmd = "SELECT userid FROM \"user\" WHERE salt=:Salt";
            var parameter = new DynamicParameters(new
            {
                Salt = salt
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                return con.QueryFirstOrDefault<int?>(cmd, parameter) == null;
            }
            catch (SqlException e)
            {
                _logger.LogError(e, "Error while checking salt");
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError,
                    "Error while checking if salt exists", e);
            }
        }
    }
}
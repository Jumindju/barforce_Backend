using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Helper.Middleware;
using Barforce_Backend.Model.User;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Barforce_Backend.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ILogger _logger;
        private readonly IHashHelper _hashHelper;
        private readonly IDbHelper _dbHelper;

        public UserRepository(ILoggerFactory loggerFactory, IHashHelper hashHelper, IDbHelper dbHelper)
        {
            _logger = loggerFactory.CreateLogger<UserRepository>();
            _hashHelper = hashHelper;
            _dbHelper = dbHelper;
        }

        public async Task Register(UserRegister newUser)
        {
            if (newUser == null)
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "No userData send");
            var validationContext = new ValidationContext(newUser);
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(newUser, validationContext, results, true))
            {
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest,
                    string.Join(',', results.Select(res => res.ErrorMessage)));
            }

            if (await UsernameExists(newUser.UserName))
            {
                throw new HttpStatusCodeException(HttpStatusCode.Conflict, "Username already exists");
            }

            var salt = await _hashHelper.CreateSalt();
            var hashedPassword = _hashHelper.GetHash(newUser.Password, salt);
            const string cmd =
                "INSERT INTO \"user\"(username, birthday, weight, password, salt, gender) VALUES (:userName,:birthday, :weight, :password, :salt, :gender)";
            var parameter = new DynamicParameters(new
            {
                newUser.UserName,
                newUser.Birthday,
                newUser.Weight,
                password = hashedPassword,
                salt,
                newUser.Gender
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                con.Execute(cmd, parameter);
                _logger.LogInformation("Created user");
            }
            catch (SqlException e)
            {
                const string errMsg = "Error while inserting user";
                _logger.LogError(e, errMsg);
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, errMsg, e);
            }
        }

        public void Verify(Guid verifyGuid)
        {
        }

        public async Task<bool> UsernameExists(string userName)
        {
            const string cmd = "SELECT userid FROM \"user\" WHERE username=:userName";
            var parameter = new DynamicParameters(new
            {
                userName
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                return con.QueryFirstOrDefault<int?>(cmd, parameter) != null;
            }
            catch (SqlException e)
            {
                const string errMsg = "Error while checking if userName exists";
                _logger.LogError(e, errMsg);
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, errMsg, e);
            }
        }
    }
}
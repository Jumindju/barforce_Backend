using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Configuration;
using Barforce_Backend.Model.Helper.Middleware;
using Barforce_Backend.Model.User;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Barforce_Backend.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ILogger _logger;
        private readonly IHashHelper _hashHelper;
        private readonly IDbHelper _dbHelper;
        private readonly ITokenHelper _tokenHelper;
        private readonly IEmailHelper _emailHelper;

        public UserRepository(
            ILoggerFactory loggerFactory,
            IHashHelper hashHelper,
            IDbHelper dbHelper,
            ITokenHelper tokenHelper,
            IEmailHelper emailHelper
        )
        {
            _logger = loggerFactory.CreateLogger<UserRepository>();
            _hashHelper = hashHelper;
            _dbHelper = dbHelper;
            _tokenHelper = tokenHelper;
            _emailHelper = emailHelper;
        }

        public async Task Register(UserRegister newUser)
        {
            if (newUser == null)
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "No userData send");

            if (await UsernameExists(newUser.UserName))
            {
                throw new HttpStatusCodeException(HttpStatusCode.Conflict, "Username already exists");
            }

            if (await EMailExists(newUser.EMail))
            {
                throw new HttpStatusCodeException(HttpStatusCode.Conflict, "User with that email already exists");
            }

            var salt = await _hashHelper.CreateSalt();
            var hashedPassword = _hashHelper.GetHash(newUser.Password, salt);
            var rndVerifier = await GetUserValidationNumber();
            const string cmd =
                "INSERT INTO \"user\"(username, birthday, weight, password, salt, gender, email, verified) VALUES (:userName,:birthday, :weight, :password, :salt, :gender, :email, :rndVerifier)";
            var parameter = new DynamicParameters(new
            {
                newUser.UserName,
                newUser.Birthday,
                newUser.Weight,
                password = hashedPassword,
                salt,
                newUser.Gender,
                newUser.EMail,
                rndVerifier
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                await con.ExecuteAsync(cmd, parameter);
                _logger.LogInformation("Created user");
                await _emailHelper.SendVerifyMail(newUser.EMail, rndVerifier);
            }
            catch (SqlException e)
            {
                const string errMsg = "Error while inserting user";
                _logger.LogError(e, errMsg);
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, errMsg, e);
            }
        }

        public async Task<string> Login(string userName, string password)
        {
            var user = await ReadUserByName(userName);
            if (user == null)
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "No user found with this name");
            if (!_hashHelper.IsCorrectPassword(password, user.Salt, user.Password))
                throw new HttpStatusCodeException(HttpStatusCode.Forbidden, "Invalid password");
            if (user.Verified != null)
                throw new HttpStatusCodeException(HttpStatusCode.NoContent, "User didnt verified his email");

            return await _tokenHelper.GetUserToken(user);
        }

        public async Task ResetPassword(int userId, string newPassword)
        {
            var user = await ReadUserById(userId);
            if (user == null)
                throw new HttpStatusCodeException(HttpStatusCode.Unauthorized, "User does not exists");
            var newSalt = await _hashHelper.CreateSalt();
            var newHashedPw = _hashHelper.GetHash(newPassword, newSalt);
            const string cmd = "UPDATE \"user\" SET password=:newPw, salt=:newSalt WHERE userid=:userId";
            var parameter = new DynamicParameters(new
            {
                userId,
                newPw = newHashedPw,
                newSalt
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                await con.ExecuteAsync(cmd, parameter);
            }
            catch (SqlException e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, "Error while resetting password",
                    e);
            }
        }

        public async Task<bool> UsernameExists(string userName)
        {
            if (userName == null || userName.Length < 4)
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "Username needs at least 4 digits");

            const string cmd = "SELECT userid FROM \"user\" WHERE username=:userName";
            var parameter = new DynamicParameters(new
            {
                userName
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                return await con.QueryFirstOrDefaultAsync<int?>(cmd, parameter) != null;
            }
            catch (SqlException e)
            {
                const string errMsg = "Error while checking if userName exists";
                _logger.LogError(e, errMsg);
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, errMsg, e);
            }
        }

        public async Task<bool> EMailExists(string email)
        {
            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(email))
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "Invalid email format");

            const string cmd = "SELECT userid FROM \"user\" WHERE email=:email";
            var parameter = new DynamicParameters(new
            {
                email
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                return await con.QueryFirstOrDefaultAsync<int?>(cmd, parameter) != null;
            }
            catch (SqlException e)
            {
                const string errMsg = "Error while checking if email exists";
                _logger.LogError(e, errMsg);
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, errMsg, e);
            }
        }

        public async Task<string> VerifyMail(int userId, Guid verifyToken)
        {
            var user = await ReadUserById(userId);
            if (user == null)
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "User to verify not found");
            if (user.Verified == null)
                throw new HttpStatusCodeException(HttpStatusCode.Conflict, "User already verified");
            if (user.Verified != verifyToken)
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "Invalid verify guid send");
            const string cmd = "UPDATE \"user\" SET verified=null WHERE userid=:userId";
            var parameter = new DynamicParameters(new
            {
                userId
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                await con.ExecuteAsync(cmd, parameter);
                return await _tokenHelper.GetUserToken(user);
            }
            catch (SqlException e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, "Error while verifing");
            }
        }

        public async Task LogOff(int userId)
        {
            const string cmd = "UPDATE \"user\" SET currenttoken=null WHERE userId=:userId";
            var parameter = new DynamicParameters(new
            {
                userId
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                await con.ExecuteAsync(cmd, parameter);
            }
            catch (SqlException e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, "Error while logging off", e);
            }
        }

        private async Task<UserDto> ReadUserByName(string userName)
        {
            const string cmd =
                "SELECT userid,username,email,birthday,weight,groups,gender,verified,currentToken,password, salt from viuser where username=:userName";
            var parameter = new DynamicParameters(new
            {
                userName
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                return await con.QueryFirstOrDefaultAsync<UserDto>(cmd, parameter);
            }
            catch (SqlException e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError,
                    "Error while reading user by userName", e);
            }
        }

        private async Task<UserDto> ReadUserById(int userId)
        {
            const string cmd =
                "SELECT userid,username,email,birthday,weight,groups,gender,verified,currentToken,password, salt from viuser where userid=:userId";
            var parameter = new DynamicParameters(new
            {
                userId
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                return await con.QueryFirstOrDefaultAsync<UserDto>(cmd, parameter);
            }
            catch (SqlException e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError,
                    "Error while reading user by userId", e);
            }
        }

        private async Task<int> GetUserValidationNumber()
        {
            var rnd = new Random();
            const string checkCmd = "SELECT userId FROM \"user\" WHERE verified=:rndNumber";
            for (var maxTries = 0; maxTries < 10; maxTries++)
            {
                var rndNumber = rnd.Next(10000, 99999);
                try
                {
                    using var con = await _dbHelper.GetConnection();
                    if (await con.ExecuteScalarAsync<int?>(checkCmd, new
                    {
                        rndNumber
                    }) == null)
                        return rndNumber;
                }
                catch (SqlException e)
                {
                    throw new HttpStatusCodeException(HttpStatusCode.InternalServerError,"Error while creating rnd verifier",e);
                }
            }
            throw new HttpStatusCodeException(HttpStatusCode.Conflict,"Could not get rnd num after 10 tries");
        }
    }
}
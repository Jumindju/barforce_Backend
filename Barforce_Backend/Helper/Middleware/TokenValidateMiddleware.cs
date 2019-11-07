using System;
using System.Data.SqlClient;
using System.Net;
using System.Threading.Tasks;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Model.Helper.Middleware;
using Dapper;
using Microsoft.AspNetCore.Http;

namespace Barforce_Backend.Helper.Middleware
{
    public class TokenValidateMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDbHelper _dbHelper;

        public TokenValidateMiddleware(RequestDelegate next, IDbHelper dbHelper)
        {
            _next = next;
            _dbHelper = dbHelper;
        }

        public async Task Invoke(HttpContext context)
        {
            await CheckUserBearer(context);
            await _next(context);
        }

        private async Task CheckUserBearer(HttpContext context)
        {
            var user = context.GetTokenUser();
            if (user == null)
                return;
            if (user.Exp < GetCurrentUnixTs())
                throw new HttpStatusCodeException(HttpStatusCode.Unauthorized, "Token expired");
            if (user.CurrentToken != null && !await IsTokenValid(user.UserId, user.CurrentToken.Value))
            {
                throw new HttpStatusCodeException(HttpStatusCode.Unauthorized, "Invalid token send");
            }
        }
        private static int GetCurrentUnixTs()
        {
            return (int) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        private async Task<bool> IsTokenValid(int userId, Guid verifyToken)
        {
            const string cmd = @"SELECT userid
                                    FROM viuser
                                    WHERE 
                                        userId=:userId AND
                                        currenttoken=:curToken AND
                                        currenttoken IS NOT NULL";
            var parameter = new DynamicParameters(new
            {
                userId,
                curToken = verifyToken
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                return await con.ExecuteScalarAsync<int?>(cmd, parameter) != null;
            }
            catch (SqlException e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError,
                    "Error while checking users token", e);
            }
        }
    }
}
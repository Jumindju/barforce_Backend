﻿using System;
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
        private readonly ITokenHelper _tokenHelper;

        public TokenValidateMiddleware(RequestDelegate next, IDbHelper dbHelper, ITokenHelper tokenHelper)
        {
            _next = next;
            _dbHelper = dbHelper;
            _tokenHelper = tokenHelper;
        }

        public async Task Invoke(HttpContext context)
        {
            var authHeader = context?.Request?.Headers["Authorization"];
            if (authHeader?.Count == 1)
            {
                var token = authHeader.ToString();
                if (string.IsNullOrEmpty(token))
                    throw new HttpStatusCodeException(HttpStatusCode.Unauthorized, "No bearer found");
                var tokenBegin = token.Substring(0, 6).ToLower();
                if (tokenBegin == "bearer")
                {
                    var bearerToken = token.Substring(7);
                    var user = _tokenHelper.GetUserFromToken(bearerToken);
                    if (user == null)
                        throw new HttpStatusCodeException(HttpStatusCode.Unauthorized, "User of token is invalid");
                    if (user.Exp < GetCurrentUnixTs())
                        throw new HttpStatusCodeException(HttpStatusCode.Unauthorized, "Token expired");
                    if (user.CurrentToken != null && !await IsTokenValid(user.UserId, user.CurrentToken.Value))
                    {
                        throw new HttpStatusCodeException(HttpStatusCode.Unauthorized, "Invalid token send");
                    }
                }
            }

            await _next(context);
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
                                        currenttoken=:curToken";
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
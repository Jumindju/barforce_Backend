using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Model.Configuration;
using Barforce_Backend.Model.Helper.Middleware;
using Barforce_Backend.Model.User;
using Dapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Barforce_Backend.Helper
{
    public class TokenHelper : ITokenHelper
    {
        private readonly JwtOptions _jwtOptions;
        private readonly IDbHelper _dbHelper;

        public TokenHelper(IOptions<JwtOptions> jwtOptions, IDbHelper dbHelper)
        {
            _jwtOptions = jwtOptions.Value;
            _dbHelper = dbHelper;
        }

        public async Task<string> GetUserToken(AuthUser user)
        {
            var key = Encoding.UTF8.GetBytes(_jwtOptions.Secret);
            var symmetricSecurityKey = new SymmetricSecurityKey(key);
            var signingCredentials =
                new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);

            var jtiGuid = Guid.NewGuid();
            const string cmd = "UPDATE \"user\" SET currenttoken=:jtiGuid WHERE userid=:userId";
            var parameter = new DynamicParameters(new
            {
                jtiGuid,
                user.UserId
            });
            try
            {
                using var con = await _dbHelper.GetConnection();
                await con.ExecuteAsync(cmd, parameter);
            }
            catch (SqlException e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, "Error while updating token", e);
            }

            var claims = GetUserClaims(user);
            claims.Add(new Claim("jti", jtiGuid.ToString()));
            if ((user.Groups & UserGroups.Admin) != 0)
                claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
            var token = new JwtSecurityToken(
                expires: DateTime.UtcNow.AddDays(_jwtOptions.ExpireDays),
                signingCredentials: signingCredentials,
                claims: claims,
                issuer: "barforce_tm"
            );
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        private static List<Claim> GetUserClaims(AuthUser user)
        {
            return new List<Claim>
            {
                new Claim("userId", user.UserId.ToString()),
                new Claim("userName", user.Username),
                new Claim("eMail", user.Email),
                new Claim("birthDay", user.Birthday.ToShortDateString()),
                new Claim("weight", user.Weight?.ToString() ?? ""),
                new Claim("gender", user.Gender ? "1" : "0`"),
            };
        }
    }
}
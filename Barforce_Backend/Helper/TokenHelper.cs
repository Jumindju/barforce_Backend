using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Model.Configuration;
using Barforce_Backend.Model.User;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Barforce_Backend.Helper
{
    public class TokenHelper : ITokenHelper
    {
        private readonly JwtOptions _jwtOptions;

        public TokenHelper(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }

        public string GetUserToken(AuthUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtOptions.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.Groups.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(_jwtOptions.ExpireDays),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                IssuedAt = DateTime.UtcNow,
                AdditionalHeaderClaims = GetUserPayload(user)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static Dictionary<string, object> GetUserPayload(AuthUser user)
        {
            return new Dictionary<string, object>
            {
                {"userName", user.Username},
                {"eMail", user.Email},
                {"birthDay", user.Birthday.ToShortDateString()},
                {"weight", user.Weight},
                {"gender", user.Gender},
            };
        }
    }
}
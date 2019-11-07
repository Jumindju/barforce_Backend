using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using Barforce_Backend.Model.Helper.Middleware;
using Barforce_Backend.Model.User;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Barforce_Backend.Helper
{
    public static class ExtensionMethods
    {
        public static TokenUser GetTokenUser(this HttpContext context)
        {
            var authHeader = context?.Request?.Headers["Authorization"];
            if (authHeader?.Count != 1)
                return null;
            var token = authHeader.ToString();
            if (string.IsNullOrEmpty(token))
                throw new HttpStatusCodeException(HttpStatusCode.Unauthorized, "No bearer found");

            var tokenBegin = token.Substring(0, 6).ToLower();
            if (tokenBegin != "bearer")
                return null;
            var bearerToken = token.Substring(7);
            var key = Encoding.UTF8.GetBytes("$XWK+mN2xG2%ZRVMzXcRt7r+EN+N3cc?");
            var handler = new JwtSecurityTokenHandler();
            var validations = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
            var claims = handler.ValidateToken(bearerToken, validations, out _);

            var validationUser = new TokenUser();
            var userIdClaim = claims.FindFirst("userId")?.Value;
            if (int.TryParse(userIdClaim, out var userId))
                validationUser.UserId = userId;
            var jtiClaim = claims.FindFirst("jti")?.Value;
            if (jtiClaim != null)
                validationUser.CurrentToken = new Guid(jtiClaim);
            var expClaim = claims.FindFirst("exp")?.Value;
            if (int.TryParse(expClaim, out var expDate))
                validationUser.Exp = expDate;
            return validationUser;

        }
    }
}
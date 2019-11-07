using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Barforce_Backend.Helper.Middleware
{
    public static class TokenValidateExtension
    {
        public static IApplicationBuilder UseTokenValidateMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenValidateMiddleware>();
        }
    }
}
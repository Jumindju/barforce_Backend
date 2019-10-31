using System;
using System.Threading.Tasks;
using Barforce_Backend.Model.Helper.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Barforce_Backend.Helper.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ExceptionMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<ExceptionMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (HttpStatusCodeException ex)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning(
                        "The response has already started, the http status code middleware will not be executed.");
                    throw;
                }


                await OverrideResponse(context, new ErrorResponse
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message,
                    StackTrace = ex.InnerException?.StackTrace
                }, (int) ex.StatusCode);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning(
                        "The response has already started, the http status code middleware will not be executed.");
                    throw;
                }

                const string errorMessage = "Unhandled error occured";
                _logger.LogError(ex, errorMessage);

                await OverrideResponse(context, new ErrorResponse
                {
                    Message = errorMessage,
                    InnerException = ex.Message,
                    StackTrace = ex.StackTrace
                }, 500);
            }
        }

        private async Task OverrideResponse(HttpContext context, ErrorResponse data, int statusCode)
        {
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            await context.Response.WriteAsync(JsonConvert.SerializeObject(data, serializerSettings));
        }
    }
}
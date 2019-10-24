using System;
using System.Net;

namespace Barforce_Backend.Model.Helper.Middleware
{
    public class HttpStatusCodeException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public HttpStatusCodeException(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCodeException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCodeException(HttpStatusCode statusCode, string message, Exception inner)
            : base(message, inner)
        {
            StatusCode = statusCode;
        }
    }
}
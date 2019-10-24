﻿using System;

namespace Barforce_Backend.Model.Helper.Middleware
{
    public class MiddlewareExceptionResponse
    {
        public string Message { get; set; }
        public string InnerException { get; set; }
        public string StackTrace { get; set; }
        public Guid ErrorGuid => Guid.NewGuid();
    }
}
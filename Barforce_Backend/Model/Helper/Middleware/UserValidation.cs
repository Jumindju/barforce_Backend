using System;

namespace Barforce_Backend.Model.Helper.Middleware
{
    public class UserValidation
    {
        public Guid? CurrentToken { get; set; }
        public int UserId { get; set; }
        public int Exp { get; set; }
    }
}
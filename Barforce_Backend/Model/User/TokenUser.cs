using System;

namespace Barforce_Backend.Model.User
{
    public class TokenUser
    {
        public Guid? CurrentToken { get; set; }
        public int UserId { get; set; }
        public int Exp { get; set; }
    }
}
using System;

namespace Barforce_Backend.Model.User
{
    public class UserDto : AuthUser
    {
        public int? Verified { get; set; }
        public Guid? CurrentToken { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
    }
}
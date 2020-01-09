using System;

namespace Barforce_Backend.Model.User
{
    public class AuthUser
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime Birthday { get; set; }
        public int? Weight { get; set; }
        public UserGroups Groups { get; set; }
        public Gender Gender { get; set; }
    }
}
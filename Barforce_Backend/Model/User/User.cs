using System;

namespace Barforce_Backend.Model.User
{
    public class User : UserRegister
    {
        public int UserId { get; set; }
        public UserGroups Groups { get; set; }
        public new bool Gender { get; set; }
        public new DateTime Birthday { get; set; }
    }
}
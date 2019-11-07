using System;
using System.ComponentModel.DataAnnotations;
using Barforce_Backend.Helper.CustomPropertyValidator;

namespace Barforce_Backend.Model.User
{
    public class UserRegister
    {
        [UserNameValidator]
        public string UserName { get; set; }

        [BirthdayValidator]
        public DateTime? Birthday { get; set; }
        public int? Weight { get; set; }

        [Required]
        [EmailAddress]
        public string EMail { get; set; }
        [Required]
        public bool? Gender { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$",ErrorMessage = "Password didnt match standarts")]
        public string Password { get; set; }
    }
}
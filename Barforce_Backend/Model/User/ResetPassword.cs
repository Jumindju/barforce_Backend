using System.ComponentModel.DataAnnotations;

namespace Barforce_Backend.Model.User
{
    public class ResetPassword
    {
        [Required]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$", ErrorMessage = "Password didnt match standart")]
        public string NewPassword { get; set; }
    }
}
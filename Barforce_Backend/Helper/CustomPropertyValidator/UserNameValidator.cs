using System.ComponentModel.DataAnnotations;

namespace Barforce_Backend.Helper.CustomPropertyValidator
{
    public class UserNameValidator : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var userName = (string) value;
            if (userName == null)
                return false;
            return userName.Length <= 64 && userName.Length >= 8;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var userName = (string) value;
            if (userName == null)
                return new ValidationResult("Username is null");
            if (userName.Length > 64)
                return new ValidationResult("Username is too long");
            return userName.Length < 8 ? new ValidationResult("Username is too short") : null;
        }
    }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace Barforce_Backend.Helper.CustomPropertyValidator
{
    public class BirthdayValidator: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value==null)
                return new ValidationResult("No birthday supplied");
            var birthDay = (DateTime) value;
            if(birthDay==DateTime.MinValue)
                return new ValidationResult("Invalid birthday");
            return (DateTime.UtcNow - birthDay).TotalDays * 365 < 16
                ? new ValidationResult("User is too young")
                : null;
        }
    }
}
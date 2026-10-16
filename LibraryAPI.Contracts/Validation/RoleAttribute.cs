using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Contracts.Validation
{
    public class RoleAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string role)
            {
                role = role.ToLower();
                if (role != "librarian" && role != "admin") return new ValidationResult("Role must be: librarian or admin");
                return ValidationResult.Success;
            }
            return new ValidationResult("Role must not be null");
        }
    }
}

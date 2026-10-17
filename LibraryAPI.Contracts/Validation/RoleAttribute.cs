using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Contracts.Validation
{
    public class RoleAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            string role = value?.ToString();
            if (string.IsNullOrEmpty(role)) return new ValidationResult("Role must not be null");
            if (!string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase) && !string.Equals(role, "librarian", StringComparison.OrdinalIgnoreCase))
                    return new ValidationResult("Role must be: librarian or admin", new[] { validationContext.MemberName });

            return ValidationResult.Success;
        }
    }
}

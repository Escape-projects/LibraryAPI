using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Contracts.Validation
{
    public class CurrentYearRangeAttribute : ValidationAttribute
    {
        private readonly int _minYear;

        public CurrentYearRangeAttribute(int minYear)
        {
            _minYear = minYear;
            ErrorMessage = $"Year must be between {_minYear} and current year";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is int year)
            {
                int maxYear = DateTime.UtcNow.Year;
                if (year < _minYear || year > maxYear) return new ValidationResult($"Year must be between {_minYear} and {maxYear}");
            }
            return ValidationResult.Success;
        }
    }
}

using LibraryAPI.Contracts.Validation;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Tests
{
    public class CurrentYearRangeAttributeTests
    {
        public static IEnumerable<object[]> YearCases()
        {
            int currentYear = DateTime.Now.Year;

            yield return new object[] { 1450, 2020, true };
            yield return new object[] { 1450, 1449, false };
            yield return new object[] { 1450, 1450, true };
            yield return new object[] { 1450, currentYear, true };        // ← текущий год
            yield return new object[] { 1450, currentYear + 1, false };  // ← следующий
            yield return new object[] { 1450, currentYear - 100, true }; // ← 100 лет назад
        }

        [Theory]
        [MemberData(nameof(YearCases))]
        public void IsValid_VariousValues_ReturnsExpected(int minYear, int value, bool expected)
        {
            var attr = new CurrentYearRangeAttribute(minYear);
            var result = attr.GetValidationResult(value, new ValidationContext(new object()));
            if (expected) Assert.Equal(ValidationResult.Success, result);
            else Assert.NotNull(result);
        }
    }
}

using LibraryAPI.Contracts.Validation;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Tests
{
    public class RoleAttributeTests
    {
        [Theory]
        [InlineData("admin", true)]
        [InlineData("librarian", true)]
        [InlineData("Admin", true)]
        [InlineData("LIBRARIAN", true)]
        [InlineData("user", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsValid_VariousValues_ReturnsExpected(string value, bool expected)
        {
            var attr = new RoleAttribute();
            var result = attr.GetValidationResult(value, new ValidationContext(new object()));
            if (expected) Assert.Equal(ValidationResult.Success, result);
            else Assert.NotNull(result);
        }
    }
}

using LibraryAPI.Contracts.Validation;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Contracts.DTOs.Book
{
    public class BookUpdateDTO
    {
        [Required]
        public string title { get; set; }

        [Required]
        [CurrentYearRange(1450)]
        public int year { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "authorId must be greater than 0")]
        public int authorId { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Contracts.DTOs.Author
{
    public class AuthorUpdateDTO
    {
        [Required]
        public string name { get; set; }
        [Required]
        public string country { get; set; }
    }
}

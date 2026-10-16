using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Contracts.DTOs.Author
{
    public class AuthorCreateDTO
    {
        [Required]
        public string name { get; set; }
        [Required]
        public string country { get; set; }
    }
}

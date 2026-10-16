using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Contracts.DTOs.Auth
{
    public class RegisterDTO
    {
        [Required]
        [MinLength(4)]
        public string login { get; set; }

        [Required]
        [MinLength(6)]
        public string password { get; set; }
    }
}

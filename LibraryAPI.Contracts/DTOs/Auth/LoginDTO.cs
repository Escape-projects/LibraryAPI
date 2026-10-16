using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Contracts.DTOs.Auth
{
    public class LoginDTO
    {
        [Required]
        public string login { get; set; }

        [Required]
        public string password { get; set; }

        public bool rememberMe { get; set; } = false;
    }
}

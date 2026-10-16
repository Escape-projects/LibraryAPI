using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Contracts.DTOs.Auth
{
    public class RefreshTokenDTO
    {
        [Required]
        public string refreshToken { get; set; }
    }
}

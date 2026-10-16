using LibraryAPI.Contracts.Validation;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Contracts.DTOs.Account
{
    public class AccountCreateDTO
    {
        [Required]
        [MinLength(4)]
        public string login { get; set; }

        [Required]
        [MinLength(6)]
        public string password { get; set; }

        [Required]
        [Role]
        public string role { get; set; }
    }
}

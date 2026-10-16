using LibraryAPI.Contracts.DTOs.Account;

namespace LibraryAPI.Contracts.DTOs.Auth
{
    public class AuthDTO
    {
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
        public AccountDTO account { get; set; }
    }
}

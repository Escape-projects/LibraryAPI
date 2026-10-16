using LibraryAPI.Contracts.DTOs.Auth;

namespace LibraryAPI.Services.API
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterDTO registerDto);
        Task<AuthDTO> LoginAsync(LoginDTO loginDTO);
        Task<AuthDTO> RefreshAsync(RefreshTokenDTO token);
        Task LogoutAsync(RefreshTokenDTO token);
    }
}

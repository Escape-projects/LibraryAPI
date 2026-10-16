using LibraryAPI.Models;

namespace LibraryAPI.Services.Jwt
{
    public interface IRefreshTokenService
    {
        public Task<string> GenerateAsync(int userId);
        public Task<RefreshToken?> GetByTokenAsync(string token);
        public Task RevokeAsync(RefreshToken token);
    }
}

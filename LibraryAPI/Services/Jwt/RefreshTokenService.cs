using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace LibraryAPI.Services.Jwt
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly LibraryContext library;
        private readonly ILogger<RefreshTokenService> logger;

        public RefreshTokenService(LibraryContext Library, ILogger<RefreshTokenService> Logger)
        {
            library = Library;
            logger = Logger;
        }

        public async Task<string> GenerateAsync(int accountId)
        {
            logger.LogInformation("Generating refresh token to account(id:{AccountId})", accountId);

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            using var hashingType = SHA256.Create();
            var tokenHash = Convert.ToBase64String(hashingType.ComputeHash(Encoding.UTF8.GetBytes(token)));
            library.RefreshTokens.Add(new RefreshToken { accountId = accountId, tokenHash = tokenHash, createdAt = DateTime.UtcNow, expireAt = DateTime.UtcNow.AddDays(30) });
            await library.SaveChangesAsync();

            logger.LogInformation("Generated refresh token to account(id:{AccountId})", accountId);
            return token;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            using var hashingType = SHA256.Create();
            var tokenHash = Convert.ToBase64String(hashingType.ComputeHash(Encoding.UTF8.GetBytes(token)));
            var refreshToken = await library.RefreshTokens.Include(token => token.account).FirstOrDefaultAsync(token => token.tokenHash == tokenHash && token.expireAt > DateTime.UtcNow);
            return refreshToken;
        }

        public async Task RevokeAsync(RefreshToken token)
        {
            logger.LogInformation("Revoking token(id:{RefreshTokenId}) for account(id:{AccountId})", token.id, token.accountId);

            library.RefreshTokens.Remove(token);
            await library.SaveChangesAsync();

            logger.LogInformation("Revoked Token(id:{RefreshTokenId}) for account(id:{AccountId})", token.id, token.accountId);
        }
    }
}

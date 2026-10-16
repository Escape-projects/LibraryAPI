using LibraryAPI.Models;
using LibraryAPI.Services.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Cryptography;
using System.Text;

namespace LibraryAPI.Tests
{
    public class RefreshTokenServiceTests
    {
        private LibraryContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<LibraryContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            return new LibraryContext(options);
        }

        private RefreshTokenService CreateService(LibraryContext library)
        {
            var logger = new Mock<ILogger<RefreshTokenService>>().Object;
            return new RefreshTokenService(library, logger);
        }

        [Fact]
        public async Task GenerateAsync_ValidAccountId_CreatesTokenInDb()
        {
            using var library = GetInMemoryContext();
            library.Accounts.Add(new Account { id = 1, login = "denny", role = "admin", passwordHash = "password hash"});
            await library.SaveChangesAsync();
            var service = CreateService(library);

            await service.GenerateAsync(1);
            Assert.Equal(1, await library.RefreshTokens.CountAsync());
        }

        [Fact]
        public async Task GenerateAsync_Twice_CreatesTwoTokens()
        {
            using var library = GetInMemoryContext();
            library.Accounts.Add(new Account { id = 1, login = "denny", role = "admin", passwordHash = "password hash" });
            await library.SaveChangesAsync();
            var service = CreateService(library);

            await service.GenerateAsync(1);
            await service.GenerateAsync(1);
            Assert.True(await library.RefreshTokens.CountAsync() == 2);
        }

        private static string Hash(string token) => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        [Fact]
        public async Task GetByTokenAsync_ValidToken_ReturnsRefreshToken()
        {
            using var library = GetInMemoryContext();
            library.Accounts.Add(new Account { id = 1, login = "denny", role = "admin", passwordHash = "password hash" });
            var token = "refresh token";
            library.RefreshTokens.Add(new RefreshToken { id = 1, accountId = 1, tokenHash = Hash(token), createdAt = DateTime.UtcNow, expireAt = DateTime.UtcNow.AddDays(1) });
            await library.SaveChangesAsync();
            var service = CreateService(library);

            var result = await service.GetByTokenAsync(token);
            Assert.NotNull(result);
            Assert.Equal(1, result.accountId);
            Assert.Equal(Hash(token), result.tokenHash);
        }

        [Fact]
        public async Task GetByTokenAsync_ExpiredToken_ReturnsNull()
        {
            using var library = GetInMemoryContext();
            library.Accounts.Add(new Account { id = 1, login = "denny", role = "admin", passwordHash = "password hash" });
            var token = "refresh token";
            var refreshToken = new RefreshToken { id = 1, accountId = 1, tokenHash = Hash(token), createdAt = DateTime.UtcNow.AddDays(-2), expireAt = DateTime.UtcNow.AddDays(-1) };
            library.RefreshTokens.Add(refreshToken);
            await library.SaveChangesAsync();
            var service = CreateService(library);

            var result = await service.GetByTokenAsync(token);
            Assert.Null(result);
        }

        [Fact]
        public async Task RevokeAsync_ExistingToken_RemovesFromDb()
        {
            using var library = GetInMemoryContext();
            library.Accounts.Add(new Account { id = 1, login = "denny", role = "admin", passwordHash = "password hash" });
            var token = "refresh token";
            var refreshToken = new RefreshToken { id = 1, accountId = 1, tokenHash = Hash(token), createdAt = DateTime.UtcNow.AddDays(-2), expireAt = DateTime.UtcNow.AddDays(-1) };
            library.RefreshTokens.Add(refreshToken);
            await library.SaveChangesAsync();
            var service = CreateService(library);

            await service.RevokeAsync(refreshToken);
            Assert.Equal(0, await library.RefreshTokens.CountAsync());
        }
    }
}

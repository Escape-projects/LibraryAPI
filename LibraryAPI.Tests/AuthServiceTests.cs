using LibraryAPI.Contracts.DTOs.Auth;
using LibraryAPI.Exceptions;
using LibraryAPI.Models;
using LibraryAPI.Services.API;
using LibraryAPI.Services.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryAPI.Tests
{
    public class AuthServiceTests
    {
        private LibraryContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<LibraryContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            return new LibraryContext(options);
        }

        private AuthService CreateService(LibraryContext library)
        {
            var logger = new Mock<ILogger<AuthService>>().Object;
            var jwtServiceMock = new Mock<IJwtService>();
            jwtServiceMock.Setup(x => x.Generate(It.IsAny<Account>())).Returns("mock access token");
            var refreshTokenMock = new Mock<IRefreshTokenService>();
            refreshTokenMock.Setup(x => x.GenerateAsync(It.IsAny<int>())).ReturnsAsync("mock refresh token");
            var hasherMock = new Mock<IPasswordHasher>();
            hasherMock.Setup(x => x.Hash("123456")).Returns("password hash");
            hasherMock.Setup(x => x.Verify("123456", "password hash")).Returns(true);
            return new AuthService(library, logger, jwtServiceMock.Object, refreshTokenMock.Object, hasherMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_ValidDto_CreatesAccount()
        {
            using var library = GetInMemoryContext();
            var service = CreateService(library);

            await service.RegisterAsync(new RegisterDTO { login = "denny", password = "123456"});

            Assert.True(await library.Accounts.CountAsync() > 0);
        }

        [Fact]
        public async Task RegisterAsync_LoginAlreadyTaken_ThrowsBadRequest()
        {
            using var library = GetInMemoryContext();
            library.Accounts.Add(new Account { id = 1, login = "denny", passwordHash = "password hash", role = "admin" });
            await library.SaveChangesAsync();
            var service = CreateService(library);

            await Assert.ThrowsAsync<BadRequestException>(() => service.RegisterAsync(new RegisterDTO { login = "denny", password = "123456" }));
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsAuthDto()
        {
            using var library = GetInMemoryContext();
            library.Accounts.Add(new Account { id = 1, login = "denny", passwordHash = "password hash", role = "admin" });
            await library.SaveChangesAsync();
            var service = CreateService(library);

            var result = await service.LoginAsync(new LoginDTO { login = "denny", password = "123456"});
            Assert.NotNull(result);
            Assert.Equal("mock access token", result.accessToken);
            Assert.Equal("admin", result.account.role);
            Assert.Equal("denny", result.account.login);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
        {
            using var library = GetInMemoryContext();
            library.Accounts.Add(new Account { id = 1, login = "denny", passwordHash = "password hash", role = "admin" });
            await library.SaveChangesAsync();
            var service = CreateService(library);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.LoginAsync(new LoginDTO { login = "denny", password = "1234567" }));
        }

        [Fact]
        public async Task LoginAsync_NonExistingLogin_ThrowsUnauthorized()
        {
            using var library = GetInMemoryContext();
            library.Accounts.Add(new Account { id = 1, login = "denny", passwordHash = "password hash", role = "admin" });
            await library.SaveChangesAsync();
            var service = CreateService(library);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.LoginAsync(new LoginDTO { login = "denny1", password = "123456" }));
        }

        [Fact]
        public async Task LoginAsync_RememberMeTrue_CreatesRefreshToken()
        {
            using var library = GetInMemoryContext();
            library.Accounts.Add(new Account { id = 1, login = "denny", passwordHash = "password hash", role = "admin" });
            await library.SaveChangesAsync();
            var service = CreateService(library);

            var result = await service.LoginAsync(new LoginDTO { login = "denny", password = "123456", rememberMe = true });
            Assert.NotNull(result.refreshToken);
            Assert.Equal("mock refresh token", result.refreshToken);
            Assert.Equal("admin", result.account.role);
            Assert.Equal("denny", result.account.login);
        }
    }
}

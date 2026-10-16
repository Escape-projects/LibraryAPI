using LibraryAPI.Contracts.DTOs.Account;
using LibraryAPI.Contracts.DTOs.Auth;
using LibraryAPI.Exceptions;
using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly LibraryContext library;
        private readonly ILogger<AuthService> logger;
        private readonly IJwtService jwtToken;
        private readonly IRefreshTokenService _refreshToken;
        private readonly IPasswordHasher hasher;

        public AuthService(LibraryContext Library, ILogger<AuthService> Logger, IJwtService JwtService, IRefreshTokenService RefreshTokenService, IPasswordHasher passwordHasher)
        {
            library = Library;
            logger = Logger;
            jwtToken = JwtService;
            _refreshToken = RefreshTokenService;
            hasher = passwordHasher;
        }

        public async Task RegisterAsync(RegisterDTO registerDto)
        {
            logger.LogInformation("Registering account: {Login}", registerDto.login);

            if (await library.Accounts.AnyAsync(account => account.login == registerDto.login)) throw new BadRequestException($"Login({registerDto.login}) already taken");
            var account = new Account { login = registerDto.login, passwordHash = hasher.Hash(registerDto.password) };
            library.Accounts.Add(account);
            await library.SaveChangesAsync();

            logger.LogInformation("Registered account: {Login}", registerDto.login);
        }

        public async Task<AuthDTO> LoginAsync(LoginDTO loginDTO)
        {
            logger.LogInformation("Logging in account: {Login}", loginDTO.login);

            var account = await library.Accounts.FirstOrDefaultAsync(account => account.login == loginDTO.login);
            if (account == null) throw new UnauthorizedException($"Login {loginDTO.login}: not found in db");
            if (!hasher.Verify(loginDTO.password, account.passwordHash)) throw new UnauthorizedException("Invalid password");
            var accessToken = jwtToken.Generate(account);
            var refreshToken = loginDTO.rememberMe ? await _refreshToken.GenerateAsync(account.id) : null;

            logger.LogInformation("Logged in account: {Login}", loginDTO.login);
            return new AuthDTO { accessToken = accessToken, refreshToken = refreshToken, account = new AccountDTO { id = account.id, login = account.login, role = account.role} };
        }

        public async Task<AuthDTO> RefreshAsync(RefreshTokenDTO token)
        {
            logger.LogInformation("Refreshing refresh token");

            var refreshToken = await _refreshToken.GetByTokenAsync(token.refreshToken);
            if (refreshToken == null) throw new UnauthorizedException("Refresh token not found in db or it expired");

            logger.LogInformation("Detected refresh token(id:{RefreshTokenId}) on account(id:{AccountId})", refreshToken.id, refreshToken.accountId);
            var accessToken = jwtToken.Generate(refreshToken.account);
            var newRefreshToken = await _refreshToken.GenerateAsync(refreshToken.account.id);
            await _refreshToken.RevokeAsync(refreshToken);

            logger.LogInformation("Refreshed refresh token(id:{RefreshTokenId}) at new refresh token on account(id:{AccountId})", refreshToken.id, refreshToken.accountId);
            return new AuthDTO { accessToken = accessToken, refreshToken = newRefreshToken, account = new AccountDTO { id = refreshToken.account.id, login = refreshToken.account.login, role = refreshToken.account.role } };
        }

        public async Task LogoutAsync(RefreshTokenDTO token)
        {
            logger.LogInformation("Logging out of refresh token");

            var refreshToken = await _refreshToken.GetByTokenAsync(token.refreshToken);
            if (refreshToken != null)
            {
                await _refreshToken.RevokeAsync(refreshToken);
            }

            logger.LogInformation("Logged out refresh token");
        }
    }
}

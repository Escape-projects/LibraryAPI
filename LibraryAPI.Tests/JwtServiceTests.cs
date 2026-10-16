using LibraryAPI.Models;
using LibraryAPI.Services.Jwt;
using LibraryAPI.Settings;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LibraryAPI.Tests
{
    public class JwtServiceTests
    {
        [Fact]
        public void Generate_ValidAccount_ReturnsNonEmptyString()
        {
            var service = new JwtService(Options.Create(new JwtSettings { SecretKey = "123456789012345678901234567890123456789012345678", Issuer = "issuer", Audience = "audience", LifetimeMinutes = 1 }));

            var result = service.Generate(new Account { id = 1, login = "denny", role = "admin", passwordHash = "password hash" });
            Assert.False(string.IsNullOrEmpty(result));
        }

        [Fact]
        public void Generate_ValidAccount_TokenHasSubClaim()
        {
            var service = new JwtService(Options.Create(new JwtSettings { SecretKey = "123456789012345678901234567890123456789012345678", Issuer = "issuer", Audience = "audience", LifetimeMinutes = 1 }));

            var account = new Account { id = 1, login = "denny", role = "admin", passwordHash = "password hash" };
            var result = service.Generate(account);
            var sub = new JwtSecurityTokenHandler().ReadJwtToken(result).Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value;
            Assert.Equal(account.id.ToString(), sub);
        }

        [Fact]
        public void Generate_ValidAccount_TokenHasLoginClaim()
        {
            var service = new JwtService(Options.Create(new JwtSettings { SecretKey = "123456789012345678901234567890123456789012345678", Issuer = "issuer", Audience = "audience", LifetimeMinutes = 1 }));

            var account = new Account { id = 1, login = "denny", role = "admin", passwordHash = "password hash" };
            var result = service.Generate(account);
            var login = new JwtSecurityTokenHandler().ReadJwtToken(result).Claims.First(c => c.Type == "login").Value;
            Assert.Equal(account.login, login);
        }

        [Fact]
        public void Generate_ValidAccount_TokenHasRoleClaim()
        {
            var service = new JwtService(Options.Create(new JwtSettings { SecretKey = "123456789012345678901234567890123456789012345678", Issuer = "issuer", Audience = "audience", LifetimeMinutes = 1 }));

            var account = new Account { id = 1, login = "denny", role = "admin", passwordHash = "password hash" };
            var result = service.Generate(account);
            var role = new JwtSecurityTokenHandler().ReadJwtToken(result).Claims.First(c => c.Type == ClaimTypes.Role).Value;
            Assert.Equal(account.role, role);
        }
    }
}

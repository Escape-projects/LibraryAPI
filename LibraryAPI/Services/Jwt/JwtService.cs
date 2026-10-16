using LibraryAPI.Models;
using LibraryAPI.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryAPI.Services.Jwt
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings jwtSettings;

        public JwtService(IOptions<JwtSettings> JwtSettings)
        {
            jwtSettings = JwtSettings.Value;
        }

        public string Generate(Account account)
        {
            // 1. Создать список claims
            var claims = new List<Claim>{
                new Claim(JwtRegisteredClaimNames.Sub, account.id.ToString()),
                new Claim("login", account.login),
                new Claim(ClaimTypes.Role, account.role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // 2. Создать ключ подписи
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. Создать токен
            var token = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtSettings.LifetimeMinutes),
                signingCredentials: credentials
            );

            // 4. Преобразовать в строку
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

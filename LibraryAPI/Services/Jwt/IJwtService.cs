using LibraryAPI.Models;

namespace LibraryAPI.Services.Jwt
{
    public interface IJwtService
    {
        string Generate(Account account);
    }
}

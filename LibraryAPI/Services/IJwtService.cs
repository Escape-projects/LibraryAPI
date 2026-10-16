using LibraryAPI.Models;

namespace LibraryAPI.Services
{
    public interface IJwtService
    {
        string Generate(Account account);
    }
}

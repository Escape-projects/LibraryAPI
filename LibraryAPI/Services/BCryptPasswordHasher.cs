using BC = BCrypt.Net.BCrypt;

namespace LibraryAPI.Services
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => BC.HashPassword(password);
        public bool Verify(string password, string hash) => BC.Verify(password, hash);
    }
}

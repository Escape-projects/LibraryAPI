using LibraryClientApp.Models;
using System.Text.Json;

namespace LibraryClientApp.Services
{
    public class TokenFileStorage
    {
        private readonly string filePath;

        public TokenFileStorage()
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appdata, "LibraryClientApp");
            Directory.CreateDirectory(dir);
            filePath = Path.Combine(dir, "token.json");
        }

        public void Save(string refreshToken)
        {
            var token = JsonSerializer.Serialize(new TokenFile { RefreshToken = refreshToken });
            File.WriteAllText(filePath, token);
        }

        public string? Load()
        {
            if (!File.Exists(filePath)) return null;
            try { return JsonSerializer.Deserialize<TokenFile>(File.ReadAllText(filePath)).RefreshToken; }
            catch { return null; }
        }

        public void Clear()
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }
}

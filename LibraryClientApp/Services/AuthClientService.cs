using LibraryClientApp.Exceptions;
using System.Net;

namespace LibraryClientApp.Services
{
    public class AuthClientService : IAuthClientService
    {
        private readonly ApiClient apiClient;

        public AuthClientService(ApiClient ApiClient)
        {
            apiClient = ApiClient;
        }

        public async Task Login()
        {
            while (true)
            {
                Console.Clear();
                Console.Write("=== Библиотека ===\n\nлогин: ");
                var login = Console.ReadLine();
                Console.Write("пароль: ");
                var password = Console.ReadLine();

                try
                {
                    if (await apiClient.LoginAsync(login, password) != "user") { Console.WriteLine("\n***неправильный логин или пароль"); Helpers.PressAnyKeyToContinue(); continue; }
                    return;
                }
                catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                { Console.WriteLine("\n***неправильный логин или пароль"); Helpers.PressAnyKeyToContinue(); continue; }
            }
        }

        public async Task<bool> Register()
        {
            while (true)
            {
                Console.Clear();
                Console.Write("=== Библиотека ===\n\nлогин: ");
                var login = Console.ReadLine();
                if (string.IsNullOrEmpty(login) || login.Length < 4) { Console.WriteLine("***логин не может быть меньше 4 символов"); Helpers.PressAnyKeyToContinue(); continue; }

                Console.Write("пароль: ");
                var password = Console.ReadLine();
                if (string.IsNullOrEmpty(password) || password.Length < 6) { Console.WriteLine("***пароль не может быть меньше 6 символов"); Helpers.PressAnyKeyToContinue(); continue; }

                try
                {
                    await apiClient.RegisterAsync(login, password);
                    return true;
                }
                catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.BadRequest) { Console.WriteLine("\n***логин занят"); Helpers.PressAnyKeyToContinue(); continue; }
            }
        }
    }
}

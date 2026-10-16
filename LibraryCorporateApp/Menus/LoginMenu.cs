using LibraryCorporateApp.Exceptions;
using LibraryCorporateApp.Services;
using System.Net;

namespace LibraryCorporateApp.Menus
{
    public class LoginMenu
    {
        private readonly ApiClient apiClient;
        private readonly LibrarianMenu librarianMenu;
        private readonly AdminMenu adminMenu;

        public LoginMenu(ApiClient ApiClient, LibrarianMenu LibrarianMenu, AdminMenu AdminMenu)
        {
            apiClient = ApiClient;
            librarianMenu = LibrarianMenu;
            adminMenu = AdminMenu;
        }

        public async Task Run()
        {
            switch (await Login())
            {
                case "librarian":
                    await librarianMenu.Run();
                    break;
                case "admin":
                    await adminMenu.Run();
                    break;
            }
        }

        async Task<string> Login()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Авторизация ===\n");
                Console.Write("login: ");
                var login = Console.ReadLine();
                Console.Write("password: ");
                var password = Console.ReadLine();

                try 
                { 
                    string role = await apiClient.LoginAsync(login, password); 
                    if (role == "user") { Console.WriteLine("\n***неправильный логин или пароль"); Helpers.PressAnyKeyToContinue(); continue; }
                    return role; 
                }
                catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized) { Console.WriteLine("\n***неправильный логин или пароль"); Helpers.PressAnyKeyToContinue(); }
            }
        }
    }
}

using LibraryClientApp.Services;

namespace LibraryClientApp.Menus
{
    public class AuthMenu
    {
        private readonly ApiClient apiClient;
        private readonly BooksMenu booksMenu;
        private readonly IAuthClientService authClient;

        public AuthMenu(ApiClient ApiClient, BooksMenu BooksMenu, IAuthClientService authClientService)
        {
            apiClient = ApiClient;
            booksMenu = BooksMenu;
            authClient = authClientService;
        }

        public async Task Run()
        {
            if (await apiClient.AutoLoginAsync()) await booksMenu.Open();

            Console.Clear();
            Console.WriteLine("=== Библиотека ===\n\n1. Войти\n2. Зарегистрироваться\n");
            var choice = Console.ReadLine()?.ToLower();
            if (string.IsNullOrEmpty(choice)) return;
            switch (choice)
            {
                case "1" or "войти":
                    await authClient.Login();
                    await booksMenu.Open();
                    break;
                case "2" or "зарегистрироваться":
                    await authClient.Register();
                    break;
                default: Console.WriteLine("***неверный ввод"); Helpers.PressAnyKeyToContinue(); break;
            }
        }
    }
}

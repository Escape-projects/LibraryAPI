using LibraryAPI.Contracts.DTOs.Account;
using LibraryAPI.Contracts.DTOs.Common;
using LibraryCorporateApp.Services;

namespace LibraryCorporateApp.Menus
{
    public  class AccountsMenu
    {
        private readonly ApiClient apiClient;
        private readonly BooksMenu booksMenu;
        private readonly IAccountClientService _accounts;

        public AccountsMenu(ApiClient ApiClient, BooksMenu BooksMenu, IAccountClientService userClientService)
        {
            apiClient = ApiClient;
            booksMenu = BooksMenu;
            _accounts = userClientService;
        }

        public async Task Open()
        {
            var filters = new AccountFilterParams();
            var accounts = await apiClient.GetAccountsAsync(filters);
            while (true)
            {
                DisplayMenu(accounts);

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        if (accounts.page < accounts.totalPages)
                        {
                            filters.page = accounts.page + 1;
                            accounts = await apiClient.GetAccountsAsync(filters);
                        }
                        else { Console.WriteLine("\n***это последняя страница"); Helpers.PressAnyKeyToContinue(); }
                        break;
                    case "2":
                        if (accounts.page > 1)
                        {
                            filters.page -= 1;
                            accounts = await apiClient.GetAccountsAsync(filters);
                        }
                        else { Console.WriteLine("\n***это первая страница"); Helpers.PressAnyKeyToContinue(); }
                        break;
                    case "3":
                        Console.Write("\n3. Открыть страницу:\nномер страницы: ");
                        if (int.TryParse(Console.ReadLine(), out int page))
                        {
                            if (page > 0 && page <= accounts.totalPages) { filters.page = page; accounts = await apiClient.GetAccountsAsync(filters); }
                            else { Console.WriteLine($"\n***введите страницу от 1 до {accounts.totalPages}"); Helpers.PressAnyKeyToContinue(); }
                        }
                        else { Console.WriteLine("\n***неверный ввод"); Helpers.PressAnyKeyToContinue(); }
                        break;
                    case "4":
                        if (await _accounts.Add()) accounts = await apiClient.GetAccountsAsync(filters);
                        break;
                    case "5":
                        if (await _accounts.Update()) accounts = await apiClient.GetAccountsAsync(filters);
                        break;
                    case "6":
                        if (await _accounts.Delete())
                        {
                            if (accounts.items.Count == 1 && accounts.page > 1) filters.page--;
                            accounts = await apiClient.GetAccountsAsync(filters);
                        }
                        break;
                    case "7":
                        await booksMenu.Open(false);
                        accounts = await apiClient.GetAccountsAsync(filters);
                        break;
                    case "0": return;
                    default: Console.WriteLine("***неизвестная опция"); Helpers.PressAnyKeyToContinue(); break;
                }
            }
        }

        void DisplayMenu(PagedResult<AccountDTO> accounts)
        {
            Console.Clear();
            Console.WriteLine("=== Библиотека ===\n");

            Console.WriteLine($"| {"ID",-4} | {"Логин",-40} | {"Роль",-11} |\n" +
                               new string('-', 70));
            foreach (var account in accounts.items) Console.WriteLine($"| {account.id,-4} | {Helpers.Truncate(account.login, 40),-40} | {account.role,-11} |");
            Console.WriteLine(new string('-', 70) +
                             $"\nСтраница {accounts.page} из {accounts.totalPages} | всего аккаунтов: {accounts.totalCount}");

            Console.Write("\n1. Следующая страница" +
                          "\n2. Прошлая страница" +
                          "\n3. Открыть страницу" +
                          "\n4. Создать новый аккаунт" +
                          "\n5. Изменить аккаунт" +
                          "\n6. Удалить аккаунт" +
                          "\n7. Список книг" +
                          "\n0. Выход" +
                          "\n" +
                          "\nВыберите опцию (0-7): ");
        }
    }
}

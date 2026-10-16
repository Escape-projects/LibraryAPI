using LibraryAPI.Contracts.DTOs.Account;
using LibraryCorporateApp.Exceptions;
using LibraryCorporateApp.Models;
using System.Net;

namespace LibraryCorporateApp.Services
{
    public class AccountClientService : IAccountClientService
    {
        private readonly ApiClient apiClient;
        private readonly AppStorage storage;

        public AccountClientService(ApiClient ApiClient, AppStorage appStorage)
        {
            apiClient = ApiClient;
            storage = appStorage;
        }

        public async Task<bool> Add()
        {
            Console.Write("\n4. Создать аккаунт:\nлогин: ");
            var login = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(login)) { Console.WriteLine("\n***логин не может быть пустым"); Helpers.PressAnyKeyToContinue(); return false; }

            Console.Write("пароль: ");
            var password = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(password)) { Console.WriteLine("\n***пароль не может быть пустым"); Helpers.PressAnyKeyToContinue(); return false; }
            if (password.Length < 6) { Console.WriteLine("\n***пароль не может быть меньше 6 символов"); Helpers.PressAnyKeyToContinue(); return false; }

            Console.Write("роль (librarian или admin): ");
            var role = Console.ReadLine()?.ToLower();
            if (string.IsNullOrWhiteSpace(role)) { Console.WriteLine("\n***роль не может быть пустой"); Helpers.PressAnyKeyToContinue(); return false; }
            if (role != "librarian" && role != "admin") { Console.WriteLine("\n***роль может быть только librarian или admin"); Helpers.PressAnyKeyToContinue(); return false; }

            try
            {
                var account = await apiClient.CreateAccountAsync(login, password, role);
                Console.WriteLine($"\nАккаунт {account.login} ({account.role}) создан");
                Helpers.PressAnyKeyToContinue();
                return true;
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.BadRequest) { Console.WriteLine("\n***логин занят"); Helpers.PressAnyKeyToContinue(); return false; }
        }

        public async Task<bool> Update()
        {
            Console.Write("\n5. Обновить аккаунт:\nid аккаунта: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("\n***id должно быть числом"); Helpers.PressAnyKeyToContinue(); return false; }

            AccountDTO account;
            try { account = await apiClient.GetAccountAsync(id); }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound) { Console.WriteLine($"\n***аккаунт с id = {id} не найден"); Helpers.PressAnyKeyToContinue(); return false; }

            Console.Write($"Аккаунт {account.login} ({account.role})\nлогин: ");
            var newLogin = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(newLogin)) newLogin = account.login;

            Console.Write("пароль: ");
            var newPassword = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(newPassword)) newPassword = null;
            else if (newPassword.Length < 6) { Console.WriteLine("\n***пароль не может быть меньше 6 символов"); Helpers.PressAnyKeyToContinue(); return false; }

            Console.Write("роль: ");
            var newRole = Console.ReadLine()?.ToLower();
            if (string.IsNullOrWhiteSpace(newRole)) newRole = account.role;
            if (newRole != "librarian" && newRole != "admin") { Console.WriteLine("\n***роль может быть только librarian или admin"); Helpers.PressAnyKeyToContinue(); return false; }
            if (storage.Account.Id == account.id && newRole != "admin") { Console.WriteLine("\n***нельзя понизить себе роль"); Helpers.PressAnyKeyToContinue(); return false; }

            await apiClient.UpdateAccountAsync(account.id, new AccountUpdateDTO { login = newLogin, password = newPassword, role = newRole });
            if (newPassword == null) Console.WriteLine($"Аккаунт: {account.login} ({account.role}) | обновлен на: '{newLogin}' ({newRole}) | пароль не обновлен");
            else Console.WriteLine($"Аккаунт: {account.login} ({account.role}) | обновлен на: '{newLogin}' ({newRole}) | пароль обновлен");
            Helpers.PressAnyKeyToContinue();
            return true;
        }

        public async Task<bool> Delete()
        {
            Console.Write("\n6. Удалить аккаунт:\nid аккаунта: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("\n***id должно быть числом"); Helpers.PressAnyKeyToContinue(); return false; }
            if (storage.Account.Id == id) { Console.WriteLine("\n***нельзя удалить свой аккаунт"); Helpers.PressAnyKeyToContinue(); return false; }

            AccountDTO account;
            try { account = await apiClient.GetAccountAsync(id); }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound) { Console.WriteLine($"\n***аккаунт с id = {id} не найден"); Helpers.PressAnyKeyToContinue(); return false; }

            Console.WriteLine($"\nАккаунт {account.login} ({account.role})\nудалить аккаунт? (да/нет)");
            if (Console.ReadLine()?.ToLower() != "да") { Console.WriteLine("\n***отмена"); Helpers.PressAnyKeyToContinue(); return false; }

            await apiClient.DeleteAccountAsync(account.id);
            Console.WriteLine($"\nАккаунт {account.login} ({account.role}) удален");
            Helpers.PressAnyKeyToContinue();
            return true;
        }
    }
}

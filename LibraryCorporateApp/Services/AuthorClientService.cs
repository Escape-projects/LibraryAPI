using LibraryAPI.Contracts.DTOs.Author;
using LibraryCorporateApp.Exceptions;
using System.Net;

namespace LibraryCorporateApp.Services
{
    public class AuthorClientService : IAuthorClientService
    {
        private readonly ApiClient apiClient;

        public AuthorClientService(ApiClient ApiClient)
        {
            apiClient = ApiClient;
        }

        public async Task<AuthorDTO?> FindOrCreate(string name)
        {
            AuthorDTO author;
            try
            {
                author = await apiClient.GetAuthorAsync(name);
                return author;
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine($"\n***автор не найден\nхотите добавить нового автора: {name}? (да/нет)");
                if (Console.ReadLine()?.ToLower() != "да") { Console.WriteLine("\n***отмена операции"); Helpers.PressAnyKeyToContinue(); return null; }

                Console.Write("введите страну автора: ");
                var country = Console.ReadLine();
                author = await apiClient.CreateAuthorAsync(name, country);

                return author;
            }
        }

        public async Task<bool> Add()
        {
            Console.Write("\n4. Добавить автора:\nимя: ");
            var name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name)) { Console.WriteLine("\n***имя не может быть пустым"); Helpers.PressAnyKeyToContinue(); return false; }

            Console.Write("страна: ");
            var country = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(country)) { Console.WriteLine("\n***страна не может быть пустой"); Helpers.PressAnyKeyToContinue(); return false; }

            var author = await apiClient.CreateAuthorAsync(name, country);
            Console.WriteLine($"\nАвтор {author.name} ({author.country}) добавлен");
            Helpers.PressAnyKeyToContinue();
            return true;
        }

        public async Task<bool> Update()
        {
            Console.Write("\n5. Изменить автора:\nid автора: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("\n***id должно быть числом"); Helpers.PressAnyKeyToContinue(); return false; }

            AuthorDTO author;
            try { author = await apiClient.GetAuthorAsync(id); }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound) { Console.WriteLine($"\n***автор с id = {id} не найден"); Helpers.PressAnyKeyToContinue(); return false; }

            Console.Write($"Автор {author.name} ({author.country})\nимя: ");
            var newName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(newName)) newName = author.name;

            Console.Write("страна: ");
            var newCountry = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(newCountry)) newCountry = author.country;

            await apiClient.UpdateAuthorAsync(author.id, new AuthorUpdateDTO { name = newName, country = newCountry });
            Console.WriteLine($"Автор: {author.name} ({author.country}) | изменен на: '{newName}' ({newCountry})");
            Helpers.PressAnyKeyToContinue();
            return true;
        }

        public async Task<bool> Delete()
        {
            Console.Write("\n6. Удалить автора:\nid автора: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("\n***id должно быть числом"); Helpers.PressAnyKeyToContinue(); return false; }

            AuthorDTO author;
            try { author = await apiClient.GetAuthorAsync(id); }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound) { Console.WriteLine($"\n***автор с id = {id} не найден"); Helpers.PressAnyKeyToContinue(); return false; }

            if (author.books.Count > 0) { Console.WriteLine($"\n***у автора {author.name} кол-во книг: {author.books.Count}\n***нельзя удалить автора пока у него есть книги"); Helpers.PressAnyKeyToContinue(); return false; }

            Console.WriteLine($"\nАвтор {author.name} ({author.country})\nудалить автора? (да/нет)");
            if (Console.ReadLine()?.ToLower() != "да") { Console.WriteLine("***отмена"); Helpers.PressAnyKeyToContinue(); return false; }

            await apiClient.DeleteAuthorAsync(author.id);
            Console.WriteLine($"\nАвтор '{author.name}' ({author.country}) удален");
            Helpers.PressAnyKeyToContinue();
            return true;
        }
    }
}

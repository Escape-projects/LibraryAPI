using LibraryAPI.Contracts.DTOs.Author;
using LibraryAPI.Contracts.DTOs.Book;
using LibraryCorporateApp.Exceptions;
using System.Net;

namespace LibraryCorporateApp.Services
{
    public class BookClientService : IBookClientService
    {
        private readonly ApiClient apiClient;
        private readonly IAuthorClientService authorService;

        public BookClientService(ApiClient ApiClient, IAuthorClientService authorClientService)
        {
            apiClient = ApiClient;
            authorService = authorClientService;
        }

        public async Task<bool> Add()
        {
            Console.Write("\n4. Добавить книгу:\nназвание: ");
            var title = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(title)) { Console.WriteLine("\n***название не может быть пустым"); Helpers.PressAnyKeyToContinue(); return false; }

            Console.Write("год выпуска: ");
            if (!int.TryParse(Console.ReadLine(), out int year)) { Console.WriteLine("\n***год должен быть числом"); Helpers.PressAnyKeyToContinue(); return false; }
            if (year < 1450 || year > DateTime.UtcNow.Year) { Console.WriteLine($"\n***неверный ввод (год книги от 1450 до {DateTime.UtcNow.Year})"); Helpers.PressAnyKeyToContinue(); return false; }

            Console.Write("автор: ");
            var authorName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(authorName)) { Console.WriteLine("\n***имя автора не может быть пустым"); Helpers.PressAnyKeyToContinue(); return false; }

            var author = await authorService.FindOrCreate(authorName);
            if (author == null) return false;

            var book = await apiClient.CreateBookAsync(title, year, author.id);
            Console.WriteLine($"\nКнига '{book.title}' ({book.year}) автора: {book.authorName} добавлена");
            Helpers.PressAnyKeyToContinue();
            return true;
        }

        public async Task<bool> Update()
        {
            Console.Write("\n5. Изменить книгу:\nid книги: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("\n***id должно быть числом"); Helpers.PressAnyKeyToContinue(); return false; }

            BookDTO book;
            try { book = await apiClient.GetBookAsync(id); }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound) { Console.WriteLine($"\n***книга с id = {id} не найдена"); Helpers.PressAnyKeyToContinue(); return false; }

            Console.Write($"Книга '{book.title}' ({book.year}) автор: {book.authorName}\nназвание: ");
            var newTitle = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(newTitle)) newTitle = book.title;

            Console.Write("год выпуска: ");
            var inputYear = Console.ReadLine();
            var newYear = book.year;
            if (!string.IsNullOrWhiteSpace(inputYear))
            {
                if (!int.TryParse(inputYear, out int _newYear) || _newYear < 1450 || _newYear > DateTime.UtcNow.Year)
                { Console.WriteLine($"\n***неверный ввод (год книги от 1450 до {DateTime.UtcNow.Year})"); Helpers.PressAnyKeyToContinue(); return false; }

                newYear = _newYear;
            }

            Console.Write("автор: ");
            var authorName = Console.ReadLine();
            var author = new AuthorDTO { id = book.authorId, name = book.authorName };
            if (!string.IsNullOrWhiteSpace(authorName))
            {
                author = await authorService.FindOrCreate(authorName);
                if (author == null) return false;
            }

            await apiClient.UpdateBookAsync(book.id, new BookUpdateDTO { title = newTitle, year = newYear, authorId = author.id });
            Console.WriteLine($"Книга: '{book.title}' ({book.year}) автор: {book.authorName} | изменена на: '{newTitle}' ({newYear}) автор: {author.name}");
            Helpers.PressAnyKeyToContinue();
            return true;
        }

        public async Task<bool> Delete()
        {
            Console.Write("\n6. Удалить книгу:\nid книги: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("\n***id должно быть числом"); Helpers.PressAnyKeyToContinue(); return false; }

            BookDTO book;
            try { book = await apiClient.GetBookAsync(id); }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound) { Console.WriteLine($"\n***книга с id = {id} не найдена"); Helpers.PressAnyKeyToContinue(); return false; }

            Console.WriteLine($"\nКнига '{book.title}' ({book.year}) автор: {book.authorName}\nудалить книгу? (да/нет)");
            if (Console.ReadLine()?.ToLower() != "да") { Console.WriteLine("***отмена"); Helpers.PressAnyKeyToContinue(); return false; }

            await apiClient.DeleteBookAsync(book.id);
            Console.WriteLine($"\nКнига '{book.title}' ({book.year}) удалена");
            Helpers.PressAnyKeyToContinue();
            return true;
        }
    }
}

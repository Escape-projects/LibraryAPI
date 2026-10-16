using LibraryAPI.Contracts.DTOs.Book;
using LibraryAPI.Contracts.DTOs.Common;
using LibraryCorporateApp.Services;

namespace LibraryCorporateApp.Menus
{
    public class BooksMenu
    {
        private readonly ApiClient apiClient;
        private readonly AuthorsMenu authorsMenu;
        private readonly IBookClientService _books;

        public BooksMenu(ApiClient ApiClient, IBookClientService bookClientService, AuthorsMenu AuthorsMenu)
        {
            apiClient = ApiClient;
            authorsMenu = AuthorsMenu;
            _books = bookClientService;
        }

        public async Task Open(bool mainMenu)
        {
            var filters = new BookFilterParams();
            var books = await apiClient.GetBooksAsync(filters);
            while (true)
            {
                DisplayMenu(books, mainMenu ? "Выход" : "Назад");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1" when books.totalCount > 0:
                        if (books.page < books.totalPages)
                        {
                            filters.page = books.page + 1;
                            books = await apiClient.GetBooksAsync(filters);
                        }
                        else { Console.WriteLine("\n***это последняя страница"); Helpers.PressAnyKeyToContinue(); }
                        break;
                    case "2" when books.totalCount > 0:
                        if (books.page > 1)
                        {
                            filters.page -= 1;
                            books = await apiClient.GetBooksAsync(filters);
                        }
                        else { Console.WriteLine("\n***это первая страница"); Helpers.PressAnyKeyToContinue(); }
                        break;
                    case "3" when books.totalCount > 0:
                        Console.Write("\n3. Открыть страницу:\nномер страницы: ");
                        if (int.TryParse(Console.ReadLine(), out int page))
                        {
                            if (page > 0 && page <= books.totalPages) { filters.page = page; books = await apiClient.GetBooksAsync(filters); }
                            else { Console.WriteLine($"\n***введите страницу от 1 до {books.totalPages}"); Helpers.PressAnyKeyToContinue(); }
                        }
                        else { Console.WriteLine("\n***неверный ввод"); Helpers.PressAnyKeyToContinue(); }
                        break;
                    case "4":
                        if (await _books.Add()) books = await apiClient.GetBooksAsync(filters);
                        break;
                    case "5" when books.totalCount > 0:
                        if (await _books.Update()) books = await apiClient.GetBooksAsync(filters);
                        break;
                    case "6" when books.totalCount > 0:
                        if (await _books.Delete())
                        {
                            if (books.items.Count == 1 && books.page > 1) filters.page--;
                            books = await apiClient.GetBooksAsync(filters);
                        }
                        break;
                    case "7":
                        await authorsMenu.Open();
                        books = await apiClient.GetBooksAsync(filters);
                        break;
                    case "0": return;
                    default: Console.WriteLine("***неизвестная опция"); Helpers.PressAnyKeyToContinue(); break;
                }
            }
        }

        void DisplayMenu(PagedResult<BookDTO> books, string ExitLabel)
        {
            Console.Clear();
            Console.WriteLine("=== Библиотека ===\n");

            if (books.totalCount == 0)
            {
                Console.WriteLine("В библиотеке нет книг");

                Console.Write("\n4. Добавить книгу" +
                              "\n7. Список авторов" +
                              $"\n0. {ExitLabel}" +
                              "\n" +
                              "\nВыберите опцию (0, 4, 7): ");
            }
            else
            {
                Console.WriteLine($"| {"ID",-4} | {"Название",-40} | {"Год",-6} | {"Автор",-25} |\n" +
                                   new string('-', 90));
                foreach (var book in books.items) Console.WriteLine($"| {book.id,-4} | {Helpers.Truncate(book.title, 40),-40} | {book.year,-6} | {Helpers.Truncate(book.authorName, 25),-25} |");
                Console.WriteLine(new string('-', 90) +
                                 $"\nСтраница {books.page} из {books.totalPages} | всего книг: {books.totalCount}");

                Console.Write("\n1. Следующая страница" +
                              "\n2. Прошлая страница" +
                              "\n3. Открыть страницу" +
                              "\n4. Добавить книгу" +
                              "\n5. Изменить книгу" +
                              "\n6. Удалить книгу" +
                              "\n7. Список авторов" +
                              $"\n0. {ExitLabel}" +
                              "\n" +
                              "\nВыберите опцию (0-7): ");
            }
        }
    }
}

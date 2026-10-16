using LibraryAPI.Contracts.DTOs.Book;
using LibraryAPI.Contracts.DTOs.Common;
using LibraryClientApp.Services;

namespace LibraryClientApp.Menus
{
    public class BooksMenu
    {
        private readonly ApiClient apiClient;
        private readonly IBookClientService bookClient;
        private readonly AuthorsMenu authorsMenu;

        public BooksMenu(ApiClient ApiClient, IBookClientService bookClientService, AuthorsMenu AuthorsMenu)
        {
            apiClient = ApiClient;
            bookClient = bookClientService;
            authorsMenu = AuthorsMenu;
        }

        public async Task Open()
        {
            var filters = new BookFilterParams();
            var books = await apiClient.GetBooksAsync(filters);
            string booksFilter = "Полный список книг";
            while (true)
            {
                var fullOptions = DisplayMenu(books, booksFilter);

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1" when fullOptions:
                        if (books.page < books.totalPages)
                        {
                            filters.page = books.page + 1;
                            books = await apiClient.GetBooksAsync(filters);
                        }
                        else { Console.WriteLine("\n***это последняя страница"); Helpers.PressAnyKeyToContinue(); }
                        break;
                    case "2" when fullOptions:
                        if (books.page > 1)
                        {
                            filters.page -= 1;
                            books = await apiClient.GetBooksAsync(filters);
                        }
                        else { Console.WriteLine("\n***это первая страница"); Helpers.PressAnyKeyToContinue(); }
                        break;
                    case "3" when fullOptions:
                        Console.Write("\n3. Открыть страницу:\nномер страницы: ");
                        if (int.TryParse(Console.ReadLine(), out int page))
                        {
                            if (page > 0 && page <= books.totalPages) { filters.page = page; books = await apiClient.GetBooksAsync(filters); }
                            else { Console.WriteLine($"\n***введите страницу от 1 до {books.totalPages}"); Helpers.PressAnyKeyToContinue(); }
                        }
                        else { Console.WriteLine("\n***неверный ввод"); Helpers.PressAnyKeyToContinue(); }
                        break;
                    case "4" when fullOptions:
                        filters = bookClient.FindByTitle();
                        books = await apiClient.GetBooksAsync(filters);
                        booksFilter = "Полный список книг";
                        if (filters.title != null) booksFilter = $"Книги с названием: {filters.title}";
                        break;
                    case "5" when fullOptions:
                        filters = bookClient.FindByYear();
                        books = await apiClient.GetBooksAsync(filters);
                        booksFilter = "Полный список книг";
                        if (filters.yearFrom > 0) booksFilter = $"Книги от: {filters.yearFrom} года";
                        if (filters.yearTo > 0) booksFilter = $"Книги до: {filters.yearTo} года";
                        if (filters.yearFrom > 0 && filters.yearTo > 0) booksFilter = $"Книги от {filters.yearFrom} и до {filters.yearTo} года";
                        break;
                    case "6" when fullOptions:
                        filters = bookClient.FindByAuthor();
                        books = await apiClient.GetBooksAsync(filters);
                        booksFilter = "Полный список книг";
                        if (filters.authorName != null) booksFilter = $"Книги с автором: {filters.authorName}";
                        break;
                    case "7" when fullOptions:
                        filters = new BookFilterParams();
                        books = await apiClient.GetBooksAsync(filters);
                        booksFilter = "Полный список книг";
                        break;
                    case "8":
                        await authorsMenu.Open();
                        filters = new BookFilterParams();
                        books = await apiClient.GetBooksAsync(filters);
                        booksFilter = "Полный список книг";
                        break;
                    case "0": await apiClient.LogoutAsync(); return;
                    default: Console.WriteLine("***неизвестная опция"); Helpers.PressAnyKeyToContinue(); break;
                }
            }
        }

        bool DisplayMenu(PagedResult<BookDTO> books, string booksFilter)
        {
            Console.Clear();
            Console.WriteLine("=== Библиотека ===\n");

            if (books.totalCount == 0 && booksFilter == "Полный список книг")
            {
                Console.WriteLine("0 книг");

                Console.Write("\n7. Обновить список книг" +
                              "\n8. Список авторов" +
                              "\n0. Выход" +
                              "\n" +
                              "\nВыберите опцию (0, 7, 8): ");

                return false;
            }
            else
            {
                Console.WriteLine($"| {"ID",-4} | {"Название",-40} | {"Год",-6} | {"Автор",-25} |\n" +
                                   new string('-', 90));
                foreach (var book in books.items) Console.WriteLine($"| {book.id,-4} | {Helpers.Truncate(book.title, 40),-40} | {book.year,-6} | {Helpers.Truncate(book.authorName, 25),-25} |");
                Console.WriteLine(new string('-', 90) +
                                 $"\nСтраница {books.page} из {books.totalPages} | всего книг: {books.totalCount} | {booksFilter}");

                Console.Write("\n1. Следующая страница" +
                              "\n2. Прошлая страница" +
                              "\n3. Открыть страницу" +
                              "\n4. Поиск по названию" +
                              "\n5. Поиск по году выпуска" +
                              "\n6. Поиск по автору" +
                              "\n7. Обновить список" +
                              "\n8. Список авторов" +
                              "\n0. Выход" +
                              "\n" +
                              "\nВыберите опцию (0-8): ");

                return true;
            }
        }
    }
}

using LibraryAPI.Contracts.DTOs.Author;
using LibraryAPI.Contracts.DTOs.Common;
using LibraryClientApp.Services;

namespace LibraryClientApp.Menus
{
    public class AuthorsMenu
    {
        private readonly ApiClient apiClient;
        private readonly IAuthorClientService authorClient;

        public AuthorsMenu(ApiClient ApiClient, IAuthorClientService authorClientService)
        {
            apiClient = ApiClient;
            authorClient = authorClientService;
        }

        public async Task Open()
        {
            var filters = new AuthorFilterParams();
            var authors = await apiClient.GetAuthorsAsync(filters);
            string authorsFilter = "Полный список авторов";
            while (true)
            {
                var fullOptions = DisplayMenu(authors, authorsFilter);

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1" when fullOptions:
                        if (authors.page < authors.totalPages)
                        {
                            filters.page = authors.page + 1;
                            authors = await apiClient.GetAuthorsAsync(filters);
                        }
                        else { Console.WriteLine("\n***это последняя страница"); Helpers.PressAnyKeyToContinue(); }
                        break;
                    case "2" when fullOptions:
                        if (authors.page > 1)
                        {
                            filters.page -= 1;
                            authors = await apiClient.GetAuthorsAsync(filters);
                        }
                        else { Console.WriteLine("\n***это первая страница"); Helpers.PressAnyKeyToContinue(); }
                        break;
                    case "3" when fullOptions:
                        Console.Write("\n3. Открыть страницу:\nномер страницы: ");
                        if (int.TryParse(Console.ReadLine(), out int page))
                        {
                            if (page > 0 && page <= authors.totalPages) { filters.page = page; authors = await apiClient.GetAuthorsAsync(filters); }
                            else { Console.WriteLine($"\n***введите страницу от 1 до {authors.totalPages}"); Helpers.PressAnyKeyToContinue(); }
                        }
                        else { Console.WriteLine("\n***неверный ввод"); Helpers.PressAnyKeyToContinue(); }
                        break;
                    case "4" when fullOptions:
                        filters = authorClient.FindByName();
                        authors = await apiClient.GetAuthorsAsync(filters);
                        authorsFilter = "Полный список авторов";
                        if (filters.name != null) authorsFilter = $"Авторы с именем: {filters.name}";
                        break;
                    case "5" when fullOptions:
                        filters = authorClient.FindByCountry();
                        authors = await apiClient.GetAuthorsAsync(filters);
                        authorsFilter = "Полный список авторов";
                        if (filters.country != null) authorsFilter = $"Авторы из страны: {filters.country}";
                        break;
                    case "6" when fullOptions:
                        filters = authorClient.FindByBooksCount();
                        authors = await apiClient.GetAuthorsAsync(filters);
                        authorsFilter = "Полный список авторов";
                        if (filters.booksCountMore > 0) authorsFilter = $"Авторы у которых больше {filters.booksCountMore} книг";
                        if (filters.booksCountLess > 0) authorsFilter = $"Авторы у которых меньше {filters.booksCountLess} книг";
                        if (filters.booksCountMore > 0 && filters.booksCountLess > 0) authorsFilter = $"Авторы у которых больше {filters.booksCountMore} но меньше {filters.booksCountLess} книг";
                        break;
                    case "7":
                        filters = new AuthorFilterParams();
                        authors = await apiClient.GetAuthorsAsync(filters);
                        authorsFilter = "Полный список авторов";
                        break;
                    case "0": return;
                    default: Console.WriteLine("***неизвестная опция"); Helpers.PressAnyKeyToContinue(); break;
                }
            }
        }

        bool DisplayMenu(PagedResult<AuthorDTO> authors, string authorsFilter)
        {
            Console.Clear();
            Console.WriteLine("=== Библиотека ===\n");

            if (authors.totalCount == 0 && authorsFilter == "Полный список авторов")
            {
                Console.WriteLine("0 авторов");

                Console.Write("\n7. Обновить список" +
                              "\n0. Назад" +
                              "\n" +
                              "\nВыберите опцию (0, 7): ");

                return false;
            }
            else
            {
                Console.WriteLine($"| {"ID",-4} | {"Имя",-40} | {"Страна",-20} | {"Кол-во книг",-11} |\n" +
                                   new string('-', 90));
                foreach (var author in authors.items) Console.WriteLine($"| {author.id,-4} | {Helpers.Truncate(author.name, 40),-40} | {Helpers.Truncate(author.country, 20),-20} | {author.books.Count.ToString(),-11} |");
                Console.WriteLine(new string('-', 90) +
                                 $"\nСтраница {authors.page} из {authors.totalPages} | всего авторов: {authors.totalCount} | {authorsFilter}");

                Console.Write("\n1. Следующая страница" +
                              "\n2. Прошлая страница" +
                              "\n3. Открыть страницу" +
                              "\n4. Поиск по имени" +
                              "\n5. Поиск по стране" +
                              "\n6. Поиск по кол-ву книг" +
                              "\n7. Обновить список" +
                              "\n0. Назад" +
                              "\n" +
                              "\nВыберите опцию (0-7): ");

                return true;
            }
        }
    }
}

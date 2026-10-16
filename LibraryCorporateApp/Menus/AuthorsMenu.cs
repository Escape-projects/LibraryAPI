using LibraryAPI.Contracts.DTOs.Author;
using LibraryAPI.Contracts.DTOs.Common;
using LibraryCorporateApp.Services;

namespace LibraryCorporateApp.Menus
{
    public class AuthorsMenu
    {
        private readonly ApiClient apiClient;
        private readonly IAuthorClientService _authors;

        public AuthorsMenu(ApiClient ApiClient, IAuthorClientService authorClientService)
        {
            apiClient = ApiClient;
            _authors = authorClientService;
        }

        public async Task Open()
        {
            var filters = new AuthorFilterParams();
            var authors = await apiClient.GetAuthorsAsync(filters);
            while (true)
            {
                DisplayMenu(authors);

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1" when authors.totalCount > 0:
                        if (authors.page < authors.totalPages)
                        {
                            filters.page = authors.page + 1;
                            authors = await apiClient.GetAuthorsAsync(filters);
                        }
                        else { Console.WriteLine("\n***это последняя страница"); Helpers.PressAnyKeyToContinue(); }
                        break;
                    case "2" when authors.totalCount > 0:
                        if (authors.page > 1)
                        {
                            filters.page -= 1;
                            authors = await apiClient.GetAuthorsAsync(filters);
                        }
                        else { Console.WriteLine("\n***это первая страница"); Helpers.PressAnyKeyToContinue(); }
                        break;
                    case "3" when authors.totalCount > 0:
                        Console.Write("\n3. Открыть страницу:\nномер страницы: ");
                        if (int.TryParse(Console.ReadLine(), out int page))
                        {
                            if (page > 0 && page <= authors.totalPages) { filters.page = page; authors = await apiClient.GetAuthorsAsync(filters); }
                            else { Console.WriteLine($"\n***введите страницу от 1 до {authors.totalPages}"); Helpers.PressAnyKeyToContinue(); }
                        }
                        else { Console.WriteLine("\n***неверный ввод"); Helpers.PressAnyKeyToContinue(); }
                        break;
                    case "4":
                        if (await _authors.Add()) authors = await apiClient.GetAuthorsAsync(filters);
                        break;
                    case "5" when authors.totalCount > 0:
                        if (await _authors.Update()) authors = await apiClient.GetAuthorsAsync(filters);
                        break;
                    case "6" when authors.totalCount > 0:
                        if (await _authors.Delete())
                        {
                            if (authors.items.Count == 1 && authors.page > 1) filters.page--;
                            authors = await apiClient.GetAuthorsAsync(filters);
                        }
                        break;
                    case "0": return;
                    default: Console.WriteLine("***неизвестная опция"); Helpers.PressAnyKeyToContinue(); break;
                }
            }
        }

        void DisplayMenu(PagedResult<AuthorDTO> authors)
        {
            Console.Clear();
            Console.WriteLine("=== Библиотека ===\n");

            if (authors.totalCount == 0)
            {
                Console.WriteLine("В библиотеке нет авторов");

                Console.Write("\n4. Добавить автора" +
                              "\n0. Назад" +
                              "\n" +
                              "\nВыберите опцию (0, 4): ");
            }
            else
            {
                Console.WriteLine($"| {"ID",-4} | {"Имя",-40} | {"Страна",-20} | {"Кол-во книг",-11} |\n" +
                                   new string('-', 90));
                foreach (var author in authors.items) Console.WriteLine($"| {author.id,-4} | {Helpers.Truncate(author.name, 40),-40} | {Helpers.Truncate(author.country, 20),-20} | {author.books.Count.ToString(),-11} |");
                Console.WriteLine(new string('-', 90) +
                                 $"\nСтраница {authors.page} из {authors.totalPages} | всего авторов: {authors.totalCount}");

                Console.Write("\n1. Следующая страница" +
                              "\n2. Прошлая страница" +
                              "\n3. Открыть страницу" +
                              "\n4. Добавить автора" +
                              "\n5. Изменить автора" +
                              "\n6. Удалить автора" +
                              "\n0. Назад" +
                              "\n" +
                              "\nВыберите опцию (0-6): ");
            }
        }
    }
}

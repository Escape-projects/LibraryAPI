using LibraryAPI.Contracts.DTOs.Author;

namespace LibraryClientApp.Services
{
    public class AuthorClientService : IAuthorClientService
    {
        public AuthorFilterParams FindByName()
        {
            Console.Write("\n4. Поиск по имени\nимя: ");
            var name = Console.ReadLine()?.ToLower();
            if (string.IsNullOrEmpty(name)) { Console.WriteLine("***имя не может быть пустым"); Helpers.PressAnyKeyToContinue(); return new AuthorFilterParams(); }
            return new AuthorFilterParams { name = name };
        }

        public AuthorFilterParams FindByCountry()
        {
            Console.Write("\n5. Поиск по стране\nстрана: ");
            var country = Console.ReadLine()?.ToLower();
            if (string.IsNullOrEmpty(country)) { Console.WriteLine("***страна не может быть пустой"); Helpers.PressAnyKeyToContinue(); return new AuthorFilterParams(); }
            return new AuthorFilterParams { country = country };
        }

        public AuthorFilterParams FindByBooksCount()
        {
            Console.Write("\n6. Поиск по кол-ву книг\n\nВиды поиска:\n1. Книг больше ...\n2. Книг меньше ...\n3. Книг больше ... но меньше ...\n\nвыбор поиска (1-3): ");
            var filters = new AuthorFilterParams();
            int booksCount;
            switch (Console.ReadLine())
            {
                case "1":
                    Console.Write("\nПоиск книг больше: ");
                    if (!int.TryParse(Console.ReadLine(), out booksCount) || booksCount <= 0) { Console.WriteLine("\n***ввод должен быть числом больше 0"); Helpers.PressAnyKeyToContinue(); break; }
                    filters.booksCountMore = booksCount;
                    break;
                case "2":
                    Console.Write("\nПоиск книг меньше: ");
                    if (!int.TryParse(Console.ReadLine(), out booksCount) || booksCount <= 0) { Console.WriteLine("\n***ввод должен быть числом больше 0"); Helpers.PressAnyKeyToContinue(); break; }
                    filters.booksCountLess = booksCount;
                    break;
                case "3":
                    Console.Write("\nПоиск книг больше: ");
                    if (!int.TryParse(Console.ReadLine(), out booksCount) || booksCount <= 0) { Console.WriteLine("\n***ввод должен быть числом больше 0"); Helpers.PressAnyKeyToContinue(); break; }
                    filters.booksCountMore = booksCount;
                    Console.Write("но меньше: ");
                    if (!int.TryParse(Console.ReadLine(), out booksCount) || booksCount <= filters.booksCountMore)
                        { Console.WriteLine($"\n***ввод должен быть числом больше {filters.booksCountMore}"); Helpers.PressAnyKeyToContinue(); break; }
                    filters.booksCountLess = booksCount;
                    break;
                default: Console.WriteLine("\n***неверный ввод"); Helpers.PressAnyKeyToContinue(); break;
            }

            return filters;
        }
    }
}

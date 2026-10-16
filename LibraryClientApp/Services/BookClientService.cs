using LibraryAPI.Contracts.DTOs.Book;

namespace LibraryClientApp.Services
{
    public class BookClientService : IBookClientService
    {
        public BookFilterParams FindByTitle()
        {
            Console.Write("\n4. Поиск по названию\nназвание: ");
            var title = Console.ReadLine()?.ToLower();
            if (string.IsNullOrEmpty(title)) { Console.WriteLine("***название не может быть пустым"); Helpers.PressAnyKeyToContinue(); return new BookFilterParams(); }
            return new BookFilterParams { title = title };
        }

        public BookFilterParams FindByYear()
        {
            Console.Write("\n5. Поиск по году выпуска\n\nВиды поиска:\n1. От ... года\n2. До ... года\n3. От ... и до ... года\n\nвыбор поиска (1-3): ");
            var filters = new BookFilterParams();
            int year;
            switch (Console.ReadLine())
            {
                case "1":
                    Console.Write("\nПоиск от ... года: ");
                    if (!int.TryParse(Console.ReadLine(), out year)) { Console.WriteLine("\n***год должен быть числом"); Helpers.PressAnyKeyToContinue(); break; }
                    filters.yearFrom = year;
                    break;
                case "2":
                    Console.Write("\nПоиск до ... года: ");
                    if (!int.TryParse(Console.ReadLine(), out year)) { Console.WriteLine("\n***год должен быть числом"); Helpers.PressAnyKeyToContinue(); break; }
                    filters.yearTo = year;
                    break;
                case "3":
                    Console.Write("\nПоиск от ... года: ");
                    if (!int.TryParse(Console.ReadLine(), out year)) { Console.WriteLine("\n***год должен быть числом"); Helpers.PressAnyKeyToContinue(); break; }
                    filters.yearFrom = year;
                    Console.Write("и до ... года: ");
                    if (!int.TryParse(Console.ReadLine(), out year)) { Console.WriteLine("\n***год должен быть числом"); Helpers.PressAnyKeyToContinue(); break; }
                    filters.yearTo = year;
                    break;
                default: Console.WriteLine("\n***неверный ввод"); Helpers.PressAnyKeyToContinue(); break;
            }

            return filters;
        }

        public BookFilterParams FindByAuthor()
        {
            Console.Write("\n6. Поиск по автору\nавтор: ");
            var author = Console.ReadLine()?.ToLower();
            if (string.IsNullOrEmpty(author)) { Console.WriteLine("***автор не может быть пустым"); Helpers.PressAnyKeyToContinue(); return new BookFilterParams(); }
            return new BookFilterParams { authorName = author };
        }
    }
}

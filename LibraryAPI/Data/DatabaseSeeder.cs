using LibraryAPI.Models;
using LibraryAPI.Services;

namespace LibraryAPI.Data
{
    public class DatabaseSeeder
    {
        public static void Seed(LibraryContext db, IPasswordHasher hasher, ILogger logger)
        {
            // Seed Admin
            if (!db.Accounts.Any(a => a.role == "admin"))
            {
                db.Accounts.Add(new Account
                {
                    login = "admin",
                    passwordHash = hasher.Hash("admin123"),
                    role = "admin"
                });
                logger.LogInformation("Seeded admin account");
            }

            // Seed Librarian
            if (!db.Accounts.Any(a => a.role == "librarian"))
            {
                db.Accounts.Add(new Account
                {
                    login = "librarian",
                    passwordHash = hasher.Hash("librarian123"),
                    role = "librarian"
                });
                logger.LogInformation("Seeded librarian account");
            }

            // Seed Authors (пример — 3 автора)
            if (!db.Authors.Any())
            {
                db.Authors.AddRange(
                    new Author { name = "Лев Толстой", country = "Россия" },
                    new Author { name = "Александр Пушкин", country = "Россия" },
                    new Author { name = "Джордж Оруэлл", country = "Великобритания" }
                );
                logger.LogInformation("Seeded 3 authors");
            }

            db.SaveChanges();

            // Seed Books (после авторов — нужны их id)
            if (!db.Books.Any())
            {
                var tolstoy = db.Authors.First(a => a.name == "Лев Толстой");
                var pushkin = db.Authors.First(a => a.name == "Александр Пушкин");
                var orwell = db.Authors.First(a => a.name == "Джордж Оруэлл");

                db.Books.AddRange(
                    new Book { title = "Война и мир", year = 1869, authorId = tolstoy.id },
                    new Book { title = "Анна Каренина", year = 1877, authorId = tolstoy.id },
                    new Book { title = "Евгений Онегин", year = 1833, authorId = pushkin.id },
                    new Book { title = "Капитанская дочка", year = 1836, authorId = pushkin.id },
                    new Book { title = "1984", year = 1949, authorId = orwell.id }
                );
                logger.LogInformation("Seeded 5 books");
            }

            db.SaveChanges();
        }
    }
}

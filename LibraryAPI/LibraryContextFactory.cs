using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LibraryAPI
{
    public class LibraryContextFactory : IDesignTimeDbContextFactory<LibraryContext>
    {
        public LibraryContext CreateDbContext(string[] args) 
        {
            var optionsBuilder = new DbContextOptionsBuilder<LibraryContext>();
            optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=LibraryDB;Integrated Security=True;TrustServerCertificate=True;");
            return new LibraryContext(optionsBuilder.Options);
        }
    }
}

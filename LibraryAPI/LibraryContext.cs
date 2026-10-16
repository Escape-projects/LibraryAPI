using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI
{
    public class LibraryContext : DbContext
    {
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Account> Accounts {  get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public LibraryContext(DbContextOptions<LibraryContext> dbContextOptions) : base(dbContextOptions) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().HasIndex(unique => unique.login).IsUnique();
        }
    }
}

using LibraryAPI.Contracts.DTOs.Book;
using LibraryAPI.Exceptions;
using LibraryAPI.Models;
using LibraryAPI.Services.API;
using LibraryAPI.Services.Redis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryAPI.Tests
{
    public class BookServiceTests : IClassFixture<MappingFixture>
    {
        private readonly MappingFixture fixture;
        private LibraryContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<LibraryContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            return new LibraryContext(options);
        }

        public BookServiceTests(MappingFixture Fixture)
        {
            fixture = Fixture;
        }

        private BookService CreateService(LibraryContext library)
        {
            var mapper = fixture.mapper;
            var logger = new Mock<ILogger<BookService>>().Object;
            var cache = new Mock<ICacheService>().Object;
            return new BookService(library, mapper, logger, cache);
        }

        [Fact]
        public async Task GetBookAsync_ExistingBook_ReturnsBookDto()
        {
            using var library = GetInMemoryContext();
            library.Authors.Add(new Author { id = 1, name = "Автор", country = "Страна"});
            library.Books.Add(new Book { id = 1, title = "Книга", year = 2000, authorId = 1 });
            await library.SaveChangesAsync();
            var service = CreateService(library);

            var result = await service.GetBookAsync(1);
            Assert.NotNull(result);
            Assert.Equal("Книга", result.title);
            Assert.Equal(2000, result.year);
            Assert.Equal(1, result.authorId);
        }

        [Fact]
        public async Task GetBookAsync_NonExistingBook_ThrowsNotFoundException()
        {
            using var library = GetInMemoryContext();
            var service = CreateService(library);

            await Assert.ThrowsAsync<NotFoundException>(() => service.GetBookAsync(999));
        }

        [Fact]
        public async Task CreateBookAsync_ValidDto_ReturnsCreatedBook()
        {
            using var library = GetInMemoryContext();
            library.Authors.Add(new Author { id = 1, name = "Автор", country = "Страна"});
            library.SaveChanges();
            var service = CreateService(library);

            var result = await service.CreateBookAsync(new BookCreateDTO { title = "Книга", year = 2000, authorId = 1 });
            Assert.NotNull(result);
            Assert.Equal("Книга", result.title);
            Assert.True(result.id > 0);
            Assert.Equal(1, await library.Books.CountAsync());
        }

        [Fact]
        public async Task CreateBookAsync_NonExistingAuthor_ThrowsNotFoundException()
        {
            using var library = GetInMemoryContext();
            var service = CreateService(library);

            await Assert.ThrowsAsync<NotFoundException>(() => service.CreateBookAsync(new BookCreateDTO { title = "Книга", year = 2000 }));
        }

        [Fact]
        public async Task DeleteBookAsync_ExistingBook_DeletesFromDb()
        {
            using var library = GetInMemoryContext();
            library.Books.Add(new Book { id = 1, title = "Книга", year = 2000 });
            await library.SaveChangesAsync();
            var service = CreateService(library);

            await service.DeleteBookAsync(1);
            Assert.Equal(0, await library.Books.CountAsync());
        }

        [Fact]
        public async Task DeleteBookAsync_NonExistingBook_ThrowsNotFoundException()
        {
            using var library = GetInMemoryContext();
            var service = CreateService(library);

            await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteBookAsync(999));
        }

        [Fact]
        public async Task GetBooksAsync_FilterByYear_ReturnsFilteredBooks()
        {
            using var library = GetInMemoryContext();
            library.Authors.Add(new Author { id = 1, name = "Автор", country = "Страна" });
            library.Books.AddRange([
                new Book { id = 1, title = "Книга1", year = 2000, authorId = 1 },
                new Book { id = 2, title = "Книга2", year = 1990, authorId = 1 },
                new Book { id = 3, title = "Книга3", year = 2010, authorId = 1 }
            ]);
            await library.SaveChangesAsync();
            var service = CreateService(library);

            var result = await service.GetBooksAsync(new BookFilterParams { yearFrom = 2000 });
            Assert.Equal(2, result.items.Count);
            Assert.Equal(2, result.totalCount);
        }
    }
}

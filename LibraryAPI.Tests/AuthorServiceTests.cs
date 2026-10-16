using LibraryAPI.Contracts.DTOs.Author;
using LibraryAPI.Exceptions;
using LibraryAPI.Models;
using LibraryAPI.Services.API;
using LibraryAPI.Services.Redis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryAPI.Tests
{
    public class AuthorServiceTests : IClassFixture<MappingFixture>
    {
        private readonly MappingFixture fixture;
        private LibraryContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<LibraryContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            return new LibraryContext(options);
        }

        public AuthorServiceTests(MappingFixture Fixture)
        {
            fixture = Fixture;
        }

        private AuthorService CreateService(LibraryContext library)
        {
            var mapper = fixture.mapper;
            var logger = new Mock<ILogger<AuthorService>>().Object;
            var cache = new Mock<ICacheService>().Object;
            return new AuthorService(library, mapper, logger, cache);
        }

        [Fact]
        public async Task GetAuthorAsync_ExistingAuthor_ReturnsAuthorDto()
        {
            using var library = GetInMemoryContext();
            library.Authors.Add(new Author { id = 1, name = "Автор", country = "Страна" }); 
            await library.SaveChangesAsync();
            var service = CreateService(library);

            var result = await service.GetAuthorAsync(1);
            Assert.NotNull(result);
            Assert.Equal("Автор", result.name);
            Assert.Equal("Страна", result.country);
        }

        [Fact]
        public async Task GetAuthorAsync_NonExistingAuthor_ThrowsNotFoundException()
        {
            using var library = GetInMemoryContext();
            var service = CreateService(library);

            await Assert.ThrowsAsync<NotFoundException>( () => service.GetAuthorAsync(999) );
        }

        [Fact]
        public async Task CreateAuthorAsync_ValidDto_ReturnsCreatedAuthor()
        {
            using var library = GetInMemoryContext();
            var service = CreateService(library);

            var result = await service.CreateAuthorAsync(new AuthorCreateDTO { name = "Автор", country = "Страна"});
            Assert.NotNull(result);
            Assert.Equal("Автор", result.name);
            Assert.True(result.id > 0);
            Assert.Equal(1, await library.Authors.CountAsync());
        }

        [Fact]
        public async Task DeleteAuthorAsync_ExistingAuthor_DeletesFromDb()
        {
            using var library = GetInMemoryContext();
            library.Authors.Add(new Author { id = 1, name = "Автор", country = "Страна" });
            await library.SaveChangesAsync();
            var service = CreateService(library);

            await service.DeleteAuthorAsync(1);
            Assert.True(await library.Authors.CountAsync() == 0);
        }

        [Fact]
        public async Task DeleteAuthorAsync_NonExistingAuthor_ThrowsNotFoundException()
        {
            using var library = GetInMemoryContext();
            var service = CreateService(library);

            await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAuthorAsync(999));
        }

        [Fact]
        public async Task DeleteAuthorAsync_AuthorWithBooks_ThrowsConflictException()
        {
            using var library = GetInMemoryContext();
            library.Authors.Add(new Author { id = 1, name = "Автор", country = "Страна" });
            library.Books.Add(new Book { id = 1, title = "Книга", year = 2000, authorId = 1});
            await library.SaveChangesAsync();
            var service = CreateService(library);

            await Assert.ThrowsAsync<ConflictException>(() => service.DeleteAuthorAsync(1));
        }

        [Fact]
        public async Task GetAuthorsAsync_FilterByName_ReturnsFilteredAuthors()
        {
            using var library = GetInMemoryContext();
            library.Authors.AddRange([
                new Author { id = 1, name = "Автор1", country = "Страна1" },
                new Author { id = 2, name = "Avtor2", country = "Страна2" },
                new Author { id = 3, name = "Автор3", country = "Страна3" }
            ]);
            await library.SaveChangesAsync();
            var service = CreateService(library);

            var result = await service.GetAuthorsAsync(new AuthorFilterParams { name = "А"});
            Assert.Equal(2, result.items.Count);
            Assert.Equal(2, result.totalCount);
        }
    }
}

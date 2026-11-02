using System.Net;
using System.Net.Http.Json;
using LibraryAPI.Contracts.DTOs.Book;
using LibraryAPI.Contracts.DTOs.Common;

namespace LibraryAPI.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class BooksApiTests : ApiTestBase
{
    public BooksApiTests(ApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateBook_AsLibrarian_ReturnsCreatedBook()
    {
        using var librarian = await Factory.CreateAuthorizedClientAsync(TestAccounts.LibrarianLogin);
        var author = await librarian.CreateAuthorAsync();
        var title = $"Книга-{Guid.NewGuid():N}";

        var response = await librarian.PostAsJsonAsync("/api/books", new BookCreateDTO
        {
            title = title,
            year = 2001,
            authorId = author.id
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<BookDTO>();
        Assert.NotNull(created);
        Assert.Equal(title, created!.title);
        Assert.Equal(author.id, created.authorId);
        Assert.Equal(author.name, created.authorName);
    }

    [Fact]
    public async Task CreateBook_AsReader_ReturnsForbidden()
    {
        using var reader = await Factory.CreateAuthorizedClientAsync(TestAccounts.ReaderLogin);

        var response = await reader.PostAsJsonAsync("/api/books", new BookCreateDTO
        {
            title = "Запрещённая книга",
            year = 2000,
            authorId = 1
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_WithUnknownAuthor_ReturnsNotFound()
    {
        using var librarian = await Factory.CreateAuthorizedClientAsync(TestAccounts.LibrarianLogin);

        var response = await librarian.PostAsJsonAsync("/api/books", new BookCreateDTO
        {
            title = "Книга без автора",
            year = 2000,
            authorId = int.MaxValue
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_WithYearOutsideRange_ReturnsBadRequest()
    {
        using var librarian = await Factory.CreateAuthorizedClientAsync(TestAccounts.LibrarianLogin);
        var author = await librarian.CreateAuthorAsync();

        var response = await librarian.PostAsJsonAsync("/api/books", new BookCreateDTO
        {
            title = "Книга из будущего",
            year = DateTime.UtcNow.Year + 1,
            authorId = author.id
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ChangesTitleAndYear()
    {
        using var librarian = await Factory.CreateAuthorizedClientAsync(TestAccounts.LibrarianLogin);
        var author = await librarian.CreateAuthorAsync();
        var book = await librarian.CreateBookAsync(author.id);
        var newTitle = $"Обновлённая-{Guid.NewGuid():N}";

        var response = await librarian.PutAsJsonAsync($"/api/books/{book.id}", new BookUpdateDTO
        {
            title = newTitle,
            year = 2010,
            authorId = author.id
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var updated = await librarian.GetFromJsonAsync<BookDTO>($"/api/books/{book.id}");
        Assert.NotNull(updated);
        Assert.Equal(newTitle, updated!.title);
        Assert.Equal(2010, updated.year);
    }

    [Fact]
    public async Task DeleteBook_RemovesIt()
    {
        using var librarian = await Factory.CreateAuthorizedClientAsync(TestAccounts.LibrarianLogin);
        var author = await librarian.CreateAuthorAsync();
        var book = await librarian.CreateBookAsync(author.id);

        var delete = await librarian.DeleteAsync($"/api/books/{book.id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var get = await librarian.GetAsync($"/api/books/{book.id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task GetBooks_FilterByTitle_ReturnsOnlyMatchingBooks()
    {
        using var librarian = await Factory.CreateAuthorizedClientAsync(TestAccounts.LibrarianLogin);
        var author = await librarian.CreateAuthorAsync();
        var prefix = $"Фильтр-{Guid.NewGuid():N}";

        await librarian.CreateBookAsync(author.id, $"{prefix}-1");
        await librarian.CreateBookAsync(author.id, $"{prefix}-2");
        await librarian.CreateBookAsync(author.id, $"Другая-{Guid.NewGuid():N}");

        var page = await librarian.GetFromJsonAsync<PagedResult<BookDTO>>(
            $"/api/books?title={Uri.EscapeDataString(prefix)}&pageSize=100");

        Assert.NotNull(page);
        Assert.Equal(2, page!.totalCount);
        Assert.All(page.items, book => Assert.Contains(prefix, book.title));
    }

    [Fact]
    public async Task GetBook_WithUnknownId_ReturnsNotFound()
    {
        using var librarian = await Factory.CreateAuthorizedClientAsync(TestAccounts.LibrarianLogin);

        var response = await librarian.GetAsync($"/api/books/{int.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

using System.Net;
using System.Net.Http.Json;
using LibraryAPI.Contracts.DTOs.Author;

namespace LibraryAPI.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class AuthorsApiTests : ApiTestBase
{
    public AuthorsApiTests(ApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateAuthor_AsLibrarian_ReturnsCreatedAuthor()
    {
        using var librarian = await Factory.CreateAuthorizedClientAsync(TestAccounts.LibrarianLogin);
        var name = $"Автор-{Guid.NewGuid():N}";

        var response = await librarian.PostAsJsonAsync("/api/authors", new AuthorCreateDTO
        {
            name = name,
            country = "Тестовая страна"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var author = await response.Content.ReadFromJsonAsync<AuthorDTO>();
        Assert.NotNull(author);
        Assert.Equal(name, author!.name);
    }

    [Fact]
    public async Task CreateAuthor_AsReader_ReturnsForbidden()
    {
        using var reader = await Factory.CreateAuthorizedClientAsync(TestAccounts.ReaderLogin);

        var response = await reader.PostAsJsonAsync("/api/authors", new AuthorCreateDTO
        {
            name = "Запрещённый автор",
            country = "Тестовая страна"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAuthor_WithUnknownId_ReturnsNotFound()
    {
        using var librarian = await Factory.CreateAuthorizedClientAsync(TestAccounts.LibrarianLogin);

        var response = await librarian.GetAsync($"/api/authors/{int.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAuthor_WithBooks_ReturnsConflict()
    {
        using var librarian = await Factory.CreateAuthorizedClientAsync(TestAccounts.LibrarianLogin);
        var author = await librarian.CreateAuthorAsync();
        await librarian.CreateBookAsync(author.id);

        var response = await librarian.DeleteAsync($"/api/authors/{author.id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}

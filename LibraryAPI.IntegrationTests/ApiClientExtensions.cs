using System.Net.Http.Headers;
using System.Net.Http.Json;
using LibraryAPI.Contracts.DTOs.Auth;
using LibraryAPI.Contracts.DTOs.Author;
using LibraryAPI.Contracts.DTOs.Book;

namespace LibraryAPI.IntegrationTests;

/// <summary>
/// Короткие помощники, чтобы тесты описывали сценарий, а не повторяли
/// логин и создание тестовых данных.
/// </summary>
internal static class ApiClientExtensions
{
    public static async Task<AuthDTO> LoginAsync(this HttpClient client, string login, string password, bool rememberMe = false)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginDTO
        {
            login = login,
            password = password,
            rememberMe = rememberMe
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthDTO>())!;
    }

    public static async Task<HttpClient> CreateAuthorizedClientAsync(this ApiFactory factory, string login)
    {
        var client = factory.CreateClient();
        var auth = await client.LoginAsync(login, TestAccounts.Password);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.accessToken);
        return client;
    }

    public static async Task<AuthorDTO> CreateAuthorAsync(this HttpClient client, string? name = null, string country = "Тестовая страна")
    {
        var response = await client.PostAsJsonAsync("/api/authors", new AuthorCreateDTO
        {
            name = name ?? $"Автор-{Guid.NewGuid():N}",
            country = country
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthorDTO>())!;
    }

    public static async Task<BookDTO> CreateBookAsync(this HttpClient client, int authorId, string? title = null, int year = 2000)
    {
        var response = await client.PostAsJsonAsync("/api/books", new BookCreateDTO
        {
            title = title ?? $"Книга-{Guid.NewGuid():N}",
            year = year,
            authorId = authorId
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<BookDTO>())!;
    }
}

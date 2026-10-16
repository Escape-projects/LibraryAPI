using LibraryAPI.Contracts.DTOs.Auth;
using LibraryAPI.Contracts.DTOs.Author;
using LibraryAPI.Contracts.DTOs.Book;
using LibraryAPI.Contracts.DTOs.Common;
using LibraryClientApp.Exceptions;
using LibraryClientApp.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Json;

namespace LibraryClientApp.Services
{
    public class ApiClient
    {
        private readonly HttpClient httpClient;
        private readonly AppStorage storage;
        private readonly TokenFileStorage tokenFileStorage;

        public ApiClient(HttpClient HttpClient, AppStorage appStorage, TokenFileStorage TokenFileStorage)
        {
            httpClient = HttpClient;
            storage = appStorage;
            tokenFileStorage = TokenFileStorage;
        }

        public async Task<string> LoginAsync(string login, string password)
        {
            var response = await httpClient.PostAsJsonAsync("api/auth/login", new LoginDTO { login = login, password = password, rememberMe = true });
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
            var auth = await response.Content.ReadFromJsonAsync<AuthDTO>();
            storage.AccessToken = auth.accessToken;
            storage.Account = new LoginAccount { Id = auth.account.id, Login = auth.account.login, Role = auth.account.role };
            tokenFileStorage.Save(auth.refreshToken);
            return storage.Account.Role;
        }

        public async Task RegisterAsync(string login, string password)
        {
            var response = await httpClient.PostAsJsonAsync("api/auth/register", new RegisterDTO { login = login, password = password });
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        public async Task LogoutAsync()
        {
            var refreshToken = tokenFileStorage.Load();
            tokenFileStorage.Clear();
            storage.AccessToken = null;
            storage.Account = null;
            if (refreshToken == null) return;

            await httpClient.PostAsJsonAsync("api/auth/logout", new RefreshTokenDTO { refreshToken = refreshToken });
        }

        public async Task<bool> AutoLoginAsync()
        {
            var refreshToken = tokenFileStorage.Load();
            if (refreshToken == null) return false;

            AuthDTO auth;
            try { auth = await RefreshAsync(refreshToken); }
            catch {  return false; }

            if (auth.account.role != "user") { tokenFileStorage.Clear(); return false; }
            
            storage.AccessToken = auth.accessToken;
            storage.Account = new LoginAccount { Id = auth.account.id, Login = auth.account.login, Role = auth.account.role };
            tokenFileStorage.Save(auth.refreshToken);
            return true;
        }

        public async Task<AuthDTO> RefreshAsync(string refreshToken)
        {
            var response = await httpClient.PostAsJsonAsync("api/auth/refresh", new RefreshTokenDTO { refreshToken = refreshToken });
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
            return await response.Content.ReadFromJsonAsync<AuthDTO>();
        }

        public async Task<PagedResult<BookDTO>> GetBooksAsync(BookFilterParams? filters = null)
        {
            filters ??= new BookFilterParams();
            var query = new Dictionary<string, string?>
            {
                ["authorId"] = filters.authorId?.ToString(),
                ["authorName"] = filters.authorName,
                ["yearFrom"] = filters.yearFrom?.ToString(),
                ["yearTo"] = filters.yearTo?.ToString(),
                ["title"] = filters.title,
                ["sortBy"] = filters.sortBy,
                ["page"] = filters.page.ToString(),
                ["pageSize"] = filters.pageSize.ToString(),
            };
            var response = await httpClient.GetAsync(QueryHelpers.AddQueryString("api/books", query));
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
            return await response.Content.ReadFromJsonAsync<PagedResult<BookDTO>>();
        }

        public async Task<PagedResult<AuthorDTO>> GetAuthorsAsync(AuthorFilterParams? filters = null)
        {
            filters ??= new AuthorFilterParams();
            var query = new Dictionary<string, string?>
            {
                ["name"] = filters.name,
                ["country"] = filters.country,
                ["booksCountMore"] = filters.booksCountMore?.ToString(),
                ["booksCountLess"] = filters.booksCountLess?.ToString(),
                ["sortBy"] = filters.sortBy,
                ["page"] = filters.page.ToString(),
                ["pageSize"] = filters.pageSize.ToString(),
            };
            var response = await httpClient.GetAsync(QueryHelpers.AddQueryString("api/authors", query));
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
            return await response.Content.ReadFromJsonAsync<PagedResult<AuthorDTO>>();
        }
    }
}

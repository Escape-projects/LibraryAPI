using LibraryAPI.Contracts.DTOs.Auth;
using LibraryAPI.Contracts.DTOs.Author;
using LibraryAPI.Contracts.DTOs.Book;
using LibraryAPI.Contracts.DTOs.Common;
using LibraryAPI.Contracts.DTOs.Account;
using LibraryCorporateApp.Exceptions;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Json;
using LibraryCorporateApp.Models;

namespace LibraryCorporateApp.Services
{
    public class ApiClient
    {
        private AppStorage storage;
        private readonly HttpClient httpClient;

        public ApiClient(HttpClient HttpClient, AppStorage appStorage)
        {
            httpClient = HttpClient;
            storage = appStorage;
        }

        public async Task<string> LoginAsync(string login, string password)
        {
            var response = await httpClient.PostAsJsonAsync("api/auth/login", new LoginDTO { login = login, password = password, rememberMe = false });
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
            var auth = await response.Content.ReadFromJsonAsync<AuthDTO>();
            storage.AccessToken = auth.accessToken;
            storage.Account = new LoginAccount { Id = auth.account.id, Login = auth.account.login, Role = auth.account.role };
            return storage.Account.Role;
        }

        public async Task<PagedResult<AuthorDTO>> GetAuthorsAsync(AuthorFilterParams? filters = null)
        {
            filters ??= new AuthorFilterParams();
            var query = new Dictionary<string, string?>{
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

        public async Task<AuthorDTO> GetAuthorAsync(int id)
        {
            var response = await httpClient.GetAsync($"api/authors/{id}");
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
            return await response.Content.ReadFromJsonAsync<AuthorDTO>();
        }

        public async Task<AuthorDTO> GetAuthorAsync(string name)
        {
            var response = await httpClient.GetAsync($"api/authors/{name}");
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
            return await response.Content.ReadFromJsonAsync<AuthorDTO>();
        }

        public async Task<AuthorDTO> CreateAuthorAsync(string name, string country)
        {
            var response = await httpClient.PostAsJsonAsync("api/authors", new AuthorCreateDTO { name = name, country = country});
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
            return await response.Content.ReadFromJsonAsync<AuthorDTO>();
        }

        public async Task UpdateAuthorAsync(int id, AuthorUpdateDTO newAuthor)
        {
            var response = await httpClient.PutAsJsonAsync($"api/authors/{id}", newAuthor);
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        public async Task DeleteAuthorAsync(int id)
        {
            var response = await httpClient.DeleteAsync($"api/authors/{id}");
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        public async Task<PagedResult<BookDTO>> GetBooksAsync(BookFilterParams? filters = null)
        {
            filters ??= new BookFilterParams();
            var query = new Dictionary<string, string?>{
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

        public async Task<BookDTO> GetBookAsync(int id)
        {
            var response = await httpClient.GetAsync($"api/books/{id}");
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
            return await response.Content.ReadFromJsonAsync<BookDTO>();
        }

        public async Task<BookDTO> CreateBookAsync(string title, int year, int authorId)
        {
            var response = await httpClient.PostAsJsonAsync("api/books", new BookCreateDTO { title = title, year = year, authorId = authorId });
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
            return await response.Content.ReadFromJsonAsync<BookDTO>();
        }

        public async Task UpdateBookAsync(int id, BookUpdateDTO newBook)
        {
            var response = await httpClient.PutAsJsonAsync($"api/books/{id}", newBook);
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        public async Task DeleteBookAsync(int id)
        {
            var response = await httpClient.DeleteAsync($"api/books/{id}");
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        public async Task<PagedResult<AccountDTO>> GetAccountsAsync(AccountFilterParams? filters = null)
        {
            filters ??= new AccountFilterParams();
            var query = new Dictionary<string, string?>
            {
                ["login"] = filters.login,
                ["role"] = filters.role,
                ["sortBy"] = filters.sortBy,
                ["page"] = filters.page.ToString(),
                ["pageSize"] = filters.pageSize.ToString(),
            };
            var response = await httpClient.GetAsync(QueryHelpers.AddQueryString("api/accounts", query));
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
            return await response.Content.ReadFromJsonAsync<PagedResult<AccountDTO>>();
        }

        public async Task<AccountDTO> GetAccountAsync(int id)
        {
            var response = await httpClient.GetAsync($"api/accounts/{id}");
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
            return await response.Content.ReadFromJsonAsync<AccountDTO>();
        }

        public async Task<AccountDTO> CreateAccountAsync(string login, string password, string role)
        {
            var response = await httpClient.PostAsJsonAsync("api/accounts", new AccountCreateDTO { login = login, password = password, role = role });
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
            return await response.Content.ReadFromJsonAsync<AccountDTO>();
        }

        public async Task UpdateAccountAsync(int id, AccountUpdateDTO newAccount)
        {
            var response = await httpClient.PutAsJsonAsync($"api/accounts/{id}", newAccount);
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        public async Task DeleteAccountAsync(int id)
        {
            var response = await httpClient.DeleteAsync($"api/accounts/{id}");
            if (!response.IsSuccessStatusCode) throw new ApiException(response.StatusCode, await response.Content.ReadAsStringAsync());
        }
    }
}

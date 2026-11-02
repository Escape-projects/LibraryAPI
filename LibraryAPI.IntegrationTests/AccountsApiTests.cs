using System.Net;
using System.Net.Http.Json;
using LibraryAPI.Contracts.DTOs.Account;
using LibraryAPI.Contracts.DTOs.Common;

namespace LibraryAPI.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class AccountsApiTests : ApiTestBase
{
    public AccountsApiTests(ApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAccounts_AsAdmin_ReturnsStaffAccounts()
    {
        using var admin = await Factory.CreateAuthorizedClientAsync(TestAccounts.AdminLogin);

        var response = await admin.GetAsync("/api/accounts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<PagedResult<AccountDTO>>();
        Assert.NotNull(page);
        Assert.Contains(page!.items, account => account.login == TestAccounts.AdminLogin);
    }

    [Fact]
    public async Task GetAccounts_AsLibrarian_ReturnsForbidden()
    {
        using var librarian = await Factory.CreateAuthorizedClientAsync(TestAccounts.LibrarianLogin);

        var response = await librarian.GetAsync("/api/accounts");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAccounts_AsReader_ReturnsForbidden()
    {
        using var reader = await Factory.CreateAuthorizedClientAsync(TestAccounts.ReaderLogin);

        var response = await reader.GetAsync("/api/accounts");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteSelf_ReturnsBadRequest()
    {
        using var admin = await Factory.CreateAuthorizedClientAsync(TestAccounts.AdminLogin);
        var auth = await admin.LoginAsync(TestAccounts.AdminLogin, TestAccounts.Password);

        var response = await admin.DeleteAsync($"/api/accounts/{auth.account.id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAccount_AsAdmin_NewAccountCanLoginWithItsRole()
    {
        using var admin = await Factory.CreateAuthorizedClientAsync(TestAccounts.AdminLogin);
        var login = $"it-staff-{Guid.NewGuid():N}";

        var create = await admin.PostAsJsonAsync("/api/accounts", new AccountCreateDTO
        {
            login = login,
            password = "Secret123!",
            role = "librarian"
        });

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        using var client = Factory.CreateClient();
        var auth = await client.LoginAsync(login, "Secret123!");
        Assert.Equal("librarian", auth.account.role);
    }
}

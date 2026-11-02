using System.Net;
using System.Net.Http.Json;
using LibraryAPI.Contracts.DTOs.Auth;

namespace LibraryAPI.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class AuthApiTests : ApiTestBase
{
    public AuthApiTests(ApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetBooks_WithoutToken_ReturnsUnauthorized()
    {
        using var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/books");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAccessTokenAndRole()
    {
        using var client = Factory.CreateClient();

        var auth = await client.LoginAsync(TestAccounts.AdminLogin, TestAccounts.Password);

        Assert.False(string.IsNullOrWhiteSpace(auth.accessToken));
        Assert.Equal("admin", auth.account.role);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        using var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginDTO
        {
            login = TestAccounts.AdminLogin,
            password = "wrong-password"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_NewAccount_CanLoginAsReader()
    {
        using var client = Factory.CreateClient();
        var login = $"it-register-{Guid.NewGuid():N}";

        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterDTO
        {
            login = login,
            password = "Secret123!"
        });

        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        var auth = await client.LoginAsync(login, "Secret123!");
        Assert.Equal("user", auth.account.role);
    }

    [Fact]
    public async Task Register_DuplicateLogin_ReturnsBadRequest()
    {
        using var client = Factory.CreateClient();
        var payload = new RegisterDTO
        {
            login = $"it-dup-{Guid.NewGuid():N}",
            password = "Secret123!"
        };

        (await client.PostAsJsonAsync("/api/auth/register", payload)).EnsureSuccessStatusCode();

        var second = await client.PostAsJsonAsync("/api/auth/register", payload);

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithRememberMe_RotatesAndRevokesOldToken()
    {
        using var client = Factory.CreateClient();
        var auth = await client.LoginAsync(TestAccounts.LibrarianLogin, TestAccounts.Password, rememberMe: true);
        Assert.False(string.IsNullOrWhiteSpace(auth.refreshToken));

        var refresh = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenDTO { refreshToken = auth.refreshToken });
        refresh.EnsureSuccessStatusCode();
        var rotated = await refresh.Content.ReadFromJsonAsync<AuthDTO>();

        Assert.NotNull(rotated);
        Assert.NotEqual(auth.refreshToken, rotated!.refreshToken);

        var reuse = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenDTO { refreshToken = auth.refreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken()
    {
        using var client = Factory.CreateClient();
        var auth = await client.LoginAsync(TestAccounts.ReaderLogin, TestAccounts.Password, rememberMe: true);
        Assert.False(string.IsNullOrWhiteSpace(auth.refreshToken));

        var logout = await client.PostAsJsonAsync("/api/auth/logout", new RefreshTokenDTO { refreshToken = auth.refreshToken });
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var refresh = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenDTO { refreshToken = auth.refreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }
}

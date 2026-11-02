using LibraryAPI.Models;
using LibraryAPI.Services.Jwt;
using LibraryAPI.Services.Redis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;

namespace LibraryAPI.IntegrationTests;

/// <summary>
/// Поднимает настоящий SQL Server в Docker, запускает приложение в памяти
/// (WebApplicationFactory) и создаёт учётки для тестов.
/// Один экземпляр на всю коллекцию тестов — контейнер стартует один раз.
/// </summary>
public sealed class ApiFactory : IAsyncLifetime
{
    private const string JwtSecret = "integration-tests-only-secret-key-0123456789abcdef";

    private readonly MsSqlContainer database = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .WithDatabase("LibraryDB")
        .Build();

    private WebApplicationFactory<Program> application = null!;

    public IServiceProvider Services => application.Services;

    public HttpClient CreateClient() => application.CreateClient();

    public async Task InitializeAsync()
    {
        await database.StartAsync();

        application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            // Testing, а не Development: сидер демо-данных не запускается.
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:DefaultConnection", database.GetConnectionString());
            builder.UseSetting("JwtSettings:SecretKey", JwtSecret);

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ICacheService>();
                services.AddSingleton<ICacheService, NoOpCacheService>();
            });
        });

        await SeedAccountsAsync();
    }

    public async Task DisposeAsync()
    {
        await application.DisposeAsync();
        await database.DisposeAsync();
    }

    private async Task SeedAccountsAsync()
    {
        using var scope = application.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        if (await db.Accounts.AnyAsync())
        {
            return;
        }

        db.Accounts.AddRange(
            new Account { login = TestAccounts.AdminLogin, role = "admin", passwordHash = hasher.Hash(TestAccounts.Password) },
            new Account { login = TestAccounts.LibrarianLogin, role = "librarian", passwordHash = hasher.Hash(TestAccounts.Password) },
            new Account { login = TestAccounts.ReaderLogin, role = "user", passwordHash = hasher.Hash(TestAccounts.Password) });

        await db.SaveChangesAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "API integration";
}

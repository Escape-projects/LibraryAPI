using LibraryClientApp.Exceptions;
using LibraryClientApp.Menus;
using LibraryClientApp.Middleware;
using LibraryClientApp.Models;
using LibraryClientApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
builder.Services.AddSingleton<TokenFileStorage>();
builder.Services.AddSingleton<AppStorage>();
builder.Services.AddTransient<AuthHeaderHandler>();

builder.Services.AddScoped<AuthMenu>();
builder.Services.AddScoped<BooksMenu>();
builder.Services.AddScoped<AuthorsMenu>();

builder.Services.AddScoped<IAuthClientService, AuthClientService>();
builder.Services.AddScoped<IBookClientService, BookClientService>();
builder.Services.AddScoped<IAuthorClientService, AuthorClientService>();

builder.Services.AddHttpClient<ApiClient>(client => client.BaseAddress = new Uri("https://localhost:7119/"))
    .AddHttpMessageHandler<AuthHeaderHandler>().AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(attempt)));

var host = builder.Build();
using var scope = host.Services.CreateScope();

try
{
    var AuthMenu = scope.ServiceProvider.GetRequiredService<AuthMenu>();
    while (true)
    {
        await AuthMenu.Run();
    }
}
catch (ApiException ex)
{
    Console.WriteLine($"***ошибка api: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"***ошибка app: {ex.Message}");
}